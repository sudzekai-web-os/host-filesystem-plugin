# Плагин файловой системы

Нативный плагин LinCtrl для работы с директориями и текстовыми файлами. Поддерживает Windows и Linux. Все маршруты находятся в группе `filesystem`, имеют русские метаданные и возвращают JSON в стандартной обёртке LinCtrl.

## Сборка

Сборка использует SDK Sudzekai.LinCtrl.Assembly 0.5.0 из NuGet. Используйте Sudzekai.LinCtrl.Hosting 0.5.1. Для Windows x64:

```powershell
dotnet publish FilesystemPlugin/FilesystemPlugin.csproj -c Release -r win-x64
```

Для Linux x64 выполните `dotnet publish FilesystemPlugin/FilesystemPlugin.csproj -c Release -r linux-x64` в Linux с установленным инструментарием Native AOT. Поместите библиотеку и манифест из папки публикации в `plugins/filesystem` рядом с хостом.

## Настройки

В `appsettings.json` хоста укажите абсолютный путь единственной корневой директории:

```json
{
  "Filesystem": {
    "Root": "C:\\LinCtrl\\Files"
  }
}
```

Для Linux пример корня — `/srv/linctrl/files`. Используйте каталог, которым управляет служба. Все параметры path, source и destination принимают виртуальный путь, например `root:\Reports\notes.txt`. Поле fullName в ответах также содержит виртуальный путь. Плагин не проходит по символьным ссылкам и junction. Корневую директорию нельзя удалять или перемещать. Операции чтения доступны ролям Viewer, User и Administrator, операции записи — Administrator.

## Директории

| Метод и маршрут | Назначение |
| --- | --- |
| GET `/filesystem/directories/info?path=...` | Атрибуты, размер, количество вложенных директорий и файлов, время UTC |
| GET `/filesystem/directories/security?path=...` | ACL на Windows или UID, GID и права POSIX на Linux |
| GET `/filesystem/directories/directories?path=...` | Непосредственно вложенные директории |
| GET `/filesystem/directories/files?path=...` | Непосредственно вложенные файлы |
| POST `/filesystem/directories` | Создание, тело `{"path":"..."}` |
| DELETE `/filesystem/directories` | Удаление, тело `{"path":"...","recursive":false}` |
| POST `/filesystem/directories/copy` | Копирование дерева, тело `{"source":"...","destination":"..."}` |
| POST `/filesystem/directories/move` | Перемещение дерева, тело `{"source":"...","destination":"..."}` |

Путь назначения должен отсутствовать, родительская директория должна существовать. Перемещение директории использует операцию файловой системы и требует одного тома.

## Файлы

| Метод и маршрут | Назначение |
| --- | --- |
| GET `/filesystem/files/info?path=...` | Размер, атрибуты, расширение и время UTC |
| GET `/filesystem/files/security?path=...` | ACL на Windows или UID, GID и права POSIX на Linux |
| GET `/filesystem/files/content?path=...` | Текст файла |
| POST `/filesystem/files` | Создание, тело `{"path":"...","content":"..."}`; текст необязателен |
| POST `/filesystem/files/copy` | Копирование, тело `{"source":"...","destination":"..."}` |
| POST `/filesystem/files/move` | Перемещение, тело `{"source":"...","destination":"..."}` |
| DELETE `/filesystem/files` | Удаление, тело `{"path":"..."}` |
| PUT `/filesystem/files/content` | Полная перезапись через `File.WriteAllText`, тело `{"path":"...","content":"..."}` |
| PATCH `/filesystem/files/content` | Замена строк, тело `{"path":"...","lines":{"1":"первая строка","4":"четвёртая строка"}}` |

Номера строк начинаются с 1. Замена сохраняет исходные разделители строк и BOM. Если хотя бы одного указанного номера нет, файл остаётся без изменений.

## История изменений

Плагин файловой системы использует плагин базы данных на том же хосте. При запуске сервис истории получает JWT через `POST /auth/login`, используя `LinCtrlDatabase:Username` и `LinCtrlDatabase:Password` системной учётной записи. Порт берётся из `LinCtrl:Port`. Перед истечением срока действия JWT обновляется при следующем обращении. При ответе 401 сервис обновляет токен и повторяет запись один раз.

После успешного изменения сервис вызывает `POST /database/history`. В записи сохраняются идентификатор инициатора, время UTC, операция и пути. Содержимое файлов и токены в историю не попадают. Если системная авторизация недоступна, изменение не начинается и маршрут возвращает 503. Если запись не подтверждена после изменения, маршрут возвращает 502 с явным указанием, что файловая операция уже выполнена.

Импорт выполняется POST /filesystem/files/import с полями path и content в Base64. Экспорт выполняется GET /filesystem/files/export?path=... и возвращает Base64 в стандартной обёртке. Лимит на файл — 8 МиБ. Импорт создаёт новый файл.
