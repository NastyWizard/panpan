#! /usr/bin/ruby

require 'fileutils'

# Configuration: folder paths

folders = {
  "SHADER_ASSETS" => "assets/shaders",
  "SPRITE_ASSETS" => "assets/sprites",
  "AUDIO_ASSETS"  => "assets/audio"
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
    if /shaders/.match(filepath)
        name += "_#{ext.gsub(".","")}"
    end
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
  
  # === MAIN LOGIC ===
  
  template = File.read(TEMPLATE_PATH)
  
  folders.each do |placeholder, folder_path|
    asset_code = generate_assets_code(folder_path)
    template.gsub!("@#{placeholder}@", asset_code)
  end
  
  File.write(OUTPUT_PATH, template)
  puts "✅ Successfully wrote embedded assets to #{OUTPUT_PATH}"