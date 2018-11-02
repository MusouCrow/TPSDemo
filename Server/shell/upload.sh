basepath=$(cd `dirname $0`; cd ..; pwd)
platform=$(uname)

if [[ "$1" == "" ]]; then
    scp -r src ubuntu@193.112.102.21:/home/ubuntu/Server
elif [[ "$1" == "all" ]]; then
	scp -r . ubuntu@193.112.102.21:/home/ubuntu/Server
elif [[ "$1" == "sh" ]]; then
	scp -r shell ubuntu@193.112.102.21:/home/ubuntu/Server
fi
