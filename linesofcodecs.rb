
def lines_of_file file_path
  File.read(file_path).count("\n")
end

def lines_of_folder folder_path
  sum = 0
  Dir[File.join(folder_path, '*.cs')].each do |f|
    next if f.include? 'Components' or f.include? '/obj/' or f.include? 'Resource.Designer'

    sum += lines_of_file f;
  end
  return sum
end



def lines_of_folders_within_folder folder_path 
  lines = 0 
  dir_path = File.join(folder_path, '*/')
  dir_path = '*/' if folder_path == ''
  depth = dir_path.count '/'

  Dir[dir_path].each do |f|
    lines_of_files = lines_of_folder f
    lines_of_folders_in_folders = lines_of_folders_within_folder f

    total = lines_of_files + lines_of_folders_in_folders

    if total > 0
      #print "(#{lines_of_files}+#{lines_of_folders_in_folders})"
      print total
      print "\t" * depth
      puts "#{f}"
    end

    lines += total

    puts if depth == 1
  end

  
  return lines
end

puts lines_of_folders_within_folder ''
