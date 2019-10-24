Результаты
==========
Реализован функционал чата согласно тз.
Добавлены аккуратный останов сервера и клиента, очистка ресурсов, free-lock collections, точки расширения под добавления новых команд.

ТЗ
==
Написать чат-сервер и чат-клиент. Сервер может давать ответы на простые предустановленные вопросы, например “Как дела?” - ”хорошо”.
Сценарий:
1. при присоединении сервер спрашивает имя клиента.
2. выводим перечень возможных вопросов.
3. по команде “пока” завершаем чат.
4. команда для получения списка всех присоединенных пользователей в чате.

Пишем обе части (общение по TCP):
Реализация сервера - C#, console, dotNet core
Реализация клиента - C#, console 
