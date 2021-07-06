# Integration Information System API

## Руководство по запуску локального окруения в Docker

Неоходимо иметь заранее установленный [Docker](https://docs.docker.com/get-docker/)

Для использования хранилища (или registry) contour необходимо модифицировать **daemon.json**, который находится в папке (для WIndows пользователей) - .docker  
**C:\\{UserName}\\.docker**
Добавить строчку

`"insecure-registries": ["docker.contour.net:5000"]`

После добавления - сохранить файл и перезапустить docker

Если вы пользуетесь приложением Docker - зайти в настройки и в разделе **Docker engine** внести указанную выше строчку, после чего нажать внопку **Apply & Restart**

Есть следующие docker-compose файлы (версии из оф. хранилищ соответствуют таковым в contour):

**work.env.setup.yaml** - здесь используются образы из официальных хранилищ;

**work.env.setup.xpack.elastic.yaml** - здесь используются  образ Elastic Search из хранилища contour, RabbitMq & PostGres - образы из официальных хранилищ.

При первом запуске yaml-файла будет происходить скачивание образов и необходимо иметь vpn-подключение к сети компании иначе могут возникнуть ошибки.



Некоторые настройки ElasticSearch (убрано именование кластера и установлено discovery.type=single-node ) изменены для запуска под Windows.

Именованные кластеры вызывают ошибку при инициализации - решение этой проблемы будет описано как только его найдем.


Файлы можно переименовать в **Docker-Compose.yaml** и запускать/удалять командами:

`docker-compose up -d` (флаг -d для того чтобы контейнеры запустились в бекграунде)

`docker-compose down` (можно добавить еще флаг -v для того чтобы удалить созданые volumes)



Если названия не менять то запускать/удалять надо командами

` docker-compose -f work.env.setup.yaml up -d`

`docker-compose -f work.env.setup.yaml down`

где work.env.setup.yaml это имя yaml файла.


ППосле запуска контейнеров необходимо установить PgAdmin:

[Для Mac OS] (https://www.pgadmin.org/download/pgadmin-4-macos/) 

[Для Windows] (https://www.pgadmin.org/download/pgadmin-4-windows/)

Дальше необходимо восстановить базу из бекапа (приклад для бд з dev2 для Windows):

`C:\Program Files (x86)\pgAdmin 4\v4\runtime\pg_dump.exe" --file "C:\\Users\\<username>\\<some folder>\\dev2.backup" --host "192.168.88.94" --port "12432" --username "postgres" --verbose --role "postgres" -T "*ower*ocations" --format=c --blobs "contour"`


При работе над проектами .net core рекомендуется использовать user-secrets или средство диспетчера секретов. Оно сохраняет конфиденциальные данные во время разработки проекта .NET Core.

https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=linux

Если вы MacOS пользователь и у вас стоит Rider используйте [плагин](https://plugins.jetbrains.com/plugin/10183--net-core-user-secrets) для работы с user secrets

После установки в контекстном меню появится пункт Open project user secrets.

Файл secret.json должен выглядеть так:

```
{
  "jwt:signingKey": "L8DD$,m4>N!?N+s6-6Sb*dV~.\\RD%q'9T%s[A}NV'#K5",
  "salt": "a}~QUUsH&}hewe42{"
}
```

