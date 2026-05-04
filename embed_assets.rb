#! /usr/bin/ruby

require 'fileutils'
require 'rbconfig'

# Configuration: folder paths

ASSETS_PATH = "assets";
if(File.exist?("../../assets")) then
  ASSETS_PATH = "../../assets"
end

folders = {
  "SHADER_ASSETS" => ASSETS_PATH + "/shaders",
  "SPRITE_ASSETS" => ASSETS_PATH + "/sprites",
  "AUDIO_ASSETS"  => ASSETS_PATH + "/audio",
  "FONT_ASSETS"  => ASSETS_PATH + "/fonts"
}

# Path to your template C# file
TEMPLATE_PATH = "assets/EmbededAssets.cs.in"
OUTPUT_PATH   = "assets/generated/Assets.cs"

# === HELPERS ===

def sanitize_variable_name(filename)
  File.basename(filename, ".*").gsub(/[^a-zA-Z0-9_]/, '_')
end

def escape_csharp_string(text)
  text.gsub("\\", "\\\\").gsub("\"", "\\\"").gsub("\n", "\\n").gsub("\r", "")
end

def generate_field_from_file(filepath)
  ext = File.extname(filepath).downcase
  name = sanitize_variable_name(filepath)
  data = File.binread(filepath)

  if [".glsl", ".txt", ".cs", ".json", ".xml"].include?(ext)
    escaped = escape_csharp_string(data)
    "        public static readonly string #{name} = \"#{escaped}\";"
  else
    bytes = data.bytes
    byte_lines = bytes.each_slice(20).map do |line|
      "            " + line.map { |b| "#{b}," }.join(" ")
    end
    # Remove the final comma from the last byte
    if byte_lines.any?
      byte_lines[-1] = byte_lines[-1].sub(/,(\s*)$/, '\1') # removes last comma
    end
    "        public static readonly byte[] #{name} = new byte[] {\n#{byte_lines.join("\n")}\n        };"
  end
end

def generate_assets_code(folder)
  return "// Folder '#{folder}' not found." unless Dir.exist?(folder)

  entries = Dir.children(folder).select { |f| File.file?(File.join(folder, f)) }
  return "// No files found in #{folder}" if entries.empty?

  entries.map do |filename|
    path = File.join(folder, filename)
    generate_field_from_file(path)
  end.join("\n\n")
end

def _generate_shader(folder, shaderName)
  isMac = RbConfig::CONFIG['host_os'] =~ /darwin/i
  outputFile = shaderName.sub(".hlsl", isMac ? ".msl" : ".sprv")
  stage = shaderName.include?("frag") ? "fragment" : "vertex"
  outFileType = isMac ? "MSL" : "SPIRV"
  puts "shadercross #{File.join(folder, shaderName)} -g -o #{File.join(folder, "generated", outputFile)} -s HLSL -d #{outFileType} -t #{stage}"
  system("shadercross #{File.join(folder, shaderName)} -g -o #{File.join(folder, "generated", outputFile)} -s HLSL -d #{outFileType} -t #{stage}")
end

def generate_shaders(folder)
  genDir = File.join(folder, "generated");
  if(!Dir.exist?(genDir))
    Dir.mkdir(genDir)
  end
  #FileUtils.rm_rf(genDir)
  #Dir.mkdir(genDir)
  entries = Dir.children(folder).select { |f| File.file?(File.join(folder, f)) }
  entries.map do |filename|
    _generate_shader(folder, filename)
  end
end

# === MAIN LOGIC ===

template = File.read(TEMPLATE_PATH)


folders.each do |placeholder, folder_path|
  if placeholder == "SHADER_ASSETS" then
    generate_shaders(folder_path)
    folder_path += "/generated"
    
  end
  asset_code = generate_assets_code(folder_path)
  template.gsub!("@#{placeholder}@", asset_code)
end
if(!File.exist?(File.dirname(OUTPUT_PATH))) then
  Dir.mkdir(File.dirname(OUTPUT_PATH))
end
File.write(OUTPUT_PATH, template)
puts "✅ Successfully wrote embedded assets to #{OUTPUT_PATH}"