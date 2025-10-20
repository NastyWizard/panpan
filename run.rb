#! usr/bin/ruby

# generate assets first
system("./embed_assets.rb")

# run
system("dotnet run")

