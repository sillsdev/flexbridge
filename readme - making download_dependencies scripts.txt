(Re)building buildUpdate.sh requires ruby and https://github.com/chrisvire/BuildUpdate
Here's the command line commands I used:

cd <path to where you want to generate the update scripts>
<your path to buildupdate.rb (part of BuildUpdate repo above)>\buildupdate.rb -t bt321 -f download_dependencies_windows.sh
<your path to buildupdate.rb (part of BuildUpdate repo above)>\buildupdate.rb -t bt435 -f download_dependencies_linux.sh

Explanation:

"-t bt321" points at the Windows configuration that tracks this branch
"-t bt435" points at the Linux configuration that tracks this branch
"-f ____" gives what I want the file to be called