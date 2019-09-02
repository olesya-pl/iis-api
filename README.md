Примечание:
To run in detached mode use [supervisor](https://til.secretgeek.net/linux/supervisor.html). 
To change configuration find `IIS/publish/replication/appsettings.${ENV}.json` file and `IIS/publish/web/appsettings.${ENV}.json`. 
`${ENV}` is env name and equals to the value of env variable `ASPNETCORE_ENVIRONMENT` (in our case `Staging`)

0. Установить dotnet-sdk-2.2.* на машину.
1. Выполнить (паблиш исполняемых файлов и конфигов) [https://git.warfare-tec.com/IIS/contour-docker/blob/master/iis_api_net.sh](https://git.warfare-tec.com/IIS/contour-docker/blob/master/iis_api_net.sh)
2. Править конфигурацию, если дефолтные значения не подходят (указывают на другие сервера БД/MQ/Elastic или имеют другие креды)

Веб - publish/web/appsettings.Staging.json

Репликатор - publish/replication/appsettings.Staging.json

3. Запустить веб с помощью supervisor
```
[root@contour-run-01 web]# cat /etc/supervisord.d/iis_web.ini 
[program:iis_web]
command=/usr/bin/dotnet /home/iis/iis-api.net/IIS/publish/web/IIS.Web.dll
directory=/home/iis/iis-api.net/IIS/publish/web
Environment=ASPNETCORE_ENVIRONMENT=Staging
stdout_logfile=/var/log/iis_web.log
autostart=true
autorestart=true
user=iis
stopsignal=KILL
numprocs=1
```

или (для теста) [https://git.warfare-tec.com/IIS/contour-docker/blob/master/iis_api_net2.sh](https://git.warfare-tec.com/IIS/contour-docker/blob/master/iis_api_net2.sh)

4. Запустить репликатор с помощью supervisor
```
[iis@contour-run-01 web]$ cat /etc/supervisord.d/iis_replication.ini
[program:iis_replication]
command=/usr/bin/dotnet /home/iis/iis-api.net/IIS/publish/replication/IIS.Replication.dll server.urls=http://localhost:5005
directory=/home/iis/iis-api.net/IIS/publish/replication
stdout_logfile=/var/log/iis_replication.log
Environment=ASPNETCORE_ENVIRONMENT=Staging
autostart=true
autorestart=true
user=iis
stopsignal=KILL
numprocs=1
```

или (для теста) [https://git.warfare-tec.com/IIS/contour-docker/blob/master/iis_api_net3.sh](https://git.warfare-tec.com/IIS/contour-docker/blob/master/iis_api_net3.sh)

5. Выполнить (post запросы на web для репликации схемы и данных) [https://git.warfare-tec.com/IIS/contour-docker/blob/master/iis_api_net4.sh](https://git.warfare-tec.com/IIS/contour-docker/blob/master/iis_api_net4.sh)

