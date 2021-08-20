sudo kill $(ps aux | grep 'Tindo.Ugit.Server.dll' | awk '{print $2}')
cd /var/local
sudo rm -rf ugit/
sudo git clone https://github.com/gaufung/ugit.git
sudo dotnet publish ugit/src/Tindo.Ugit.Server/Tindo.Ugit.Server.csproj --framework net5.0 --runtime linux-x64 -c Release -o ugit/artifact
export ASPNETCORE_ENVIRONMENT=Production
cd ugit/artifact
nohup dotnet Tindo.Ugit.Server.dll &