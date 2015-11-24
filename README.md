# CleanupMegafonXml
*Очистка xml-ек, которые выдаёт Мегафон при заказе детализации в формате Excel.*

Планируется:

1. дочистить.
2. поправить xml-ку для более удобной ручной обработки в Excel:
   * Разбить построчно инфу из шапки;
   * Удалить лишние merged-колонки;
   * Удалить последние строки (Итого и т.п.), чтобы осталась только инфа о детализации в шапке, заголовочная строка таблицы и строки данных.
   * Объединить колонки Дата-Время.
   * Типизировать данные, чтобы хотя бы Excel понимал, что 02:23 это 2 минуты 23 секунды, и числа и т.п.
3. ? перегнать в sqlite какой-нибудь?
4. GUI-прога анализа, шаблоны тарифов, оценка стоимости и подбор оптимального (то, что сейчас можно сделать вручную в Excel).
