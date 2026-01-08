using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Glossolalia
{
    /// <summary>
    /// Словарь синонимов и антонимов для игры
    /// </summary>
    public class SynonymDictionary
    {
        #region Поля

        private readonly List<string[]> synonymsA;
        private readonly List<string[]> synonymsB;
        private readonly Dictionary<string, (int LineIndex, int FileId)> wordIndex;
        private readonly List<string> allWords;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор словаря синонимов
        /// </summary>
        /// <param name="pathA">Путь к файлу со словами группы A</param>
        /// <param name="pathB">Путь к файлу со словами группы B</param>
        public SynonymDictionary(string pathA, string pathB)
        {
            synonymsA = new List<string[]>();
            synonymsB = new List<string[]>();
            allWords = new List<string>();

            LoadFiles(pathA, pathB);
            wordIndex = BuildIndex();
            BuildAllWordsList();
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Получить все синонимы слова
        /// </summary>
        /// <returns>Список синонимов или пустой список, если слово не найдено</returns>
        public IReadOnlyList<string> GetSynonyms(string word)
        {
            string normalizedWord = word.ToLowerInvariant();
            if (!wordIndex.TryGetValue(normalizedWord, out var info))
                return Array.Empty<string>();

            var source = info.FileId == 0 ? synonymsA : synonymsB;
            return source[info.LineIndex];
        }

        /// <summary>
        /// Получить все антонимы слова
        /// </summary>
        /// <returns>Список антонимов или пустой список, если слово не найдено</returns>
        public IReadOnlyList<string> GetAntonyms(string word)
        {
            string normalizedWord = word.ToLowerInvariant();
            if (!wordIndex.TryGetValue(normalizedWord, out var info))
                return Array.Empty<string>();

            var source = info.FileId == 0 ? synonymsB : synonymsA;
            return source[info.LineIndex];
        }

        /// <summary>
        /// Проверить, являются ли два слова синонимами
        /// </summary>
        /// <returns>True, если слова находятся в одном файле и одной строке, иначе False</returns>
        public bool AreSynonyms(string word1, string word2)
        {
            string normalizedWord1 = word1.ToLowerInvariant();
            string normalizedWord2 = word2.ToLowerInvariant();

            if (!wordIndex.TryGetValue(normalizedWord1, out var info1) ||
                !wordIndex.TryGetValue(normalizedWord2, out var info2))
                return false;

            return info1.LineIndex == info2.LineIndex && info1.FileId == info2.FileId;
        }

        /// <summary>
        /// Проверить, являются ли два слова антонимами
        /// </summary>
        /// <returns>True, если слова находятся в разных файлах, но одной строке, иначе False</returns>
        public bool AreAntonyms(string word1, string word2)
        {
            string normalizedWord1 = word1.ToLowerInvariant();
            string normalizedWord2 = word2.ToLowerInvariant();

            if (!wordIndex.TryGetValue(normalizedWord1, out var info1) ||
                !wordIndex.TryGetValue(normalizedWord2, out var info2))
                return false;

            return info1.LineIndex == info2.LineIndex && info1.FileId != info2.FileId;
        }

        /// <summary>
        /// Получить случайное слово из словаря
        /// </summary>
        /// <returns>Случайное слово</returns>
        public string GetRandomWord(Random random)
        {
            if (allWords.Count == 0) return "СЛОВО";

            return allWords[random.Next(allWords.Count)];
        }

        /// <summary>
        /// Получить все слова из словаря
        /// </summary>
        /// <returns>Список всех слов</returns>
        public IReadOnlyList<string> GetAllWords()
        {
            return allWords.AsReadOnly();
        }

        #endregion

        #region Свойства

        /// <summary>
        /// Общее количество уникальных слов в словаре
        /// </summary>
        public int TotalWords => allWords.Count;

        /// <summary>
        /// Количество синонимических пар (строк)
        /// Одна строка включает слова из обоих файлов
        /// </summary>
        public int SynonymPairsCount => Math.Min(synonymsA.Count, synonymsB.Count);

        #endregion

        #region Приватные методы

        /// <summary>
        /// Загружает данные из двух файлов построчно
        /// </summary>
        /// <remarks>
        /// Предполагается, что файлы имеют одинаковое количество строк
        /// и строки соответствуют друг другу по номеру
        /// </remarks>
        private void LoadFiles(string pathA, string pathB)
        {
            try
            {
                using (var readerA = new StreamReader(pathA))
                using (var readerB = new StreamReader(pathB))
                {
                    string lineA, lineB;
                    // Читаем оба файла одновременно, пока есть строки в обоих файлах
                    while ((lineA = readerA.ReadLine()) != null &&
                           (lineB = readerB.ReadLine()) != null)
                    {
                        // Разбиваем строки на слова и добавляем в соответствующие списки
                        var wordsA = SplitLine(lineA);
                        var wordsB = SplitLine(lineB);

                        // Проверяем, что строки не пустые
                        if (wordsA.Length > 0 && wordsB.Length > 0)
                        {
                            synonymsA.Add(wordsA);
                            synonymsB.Add(wordsB);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // В случае ошибки загрузки словарь останется пустым
            }
        }

        /// <summary>
        /// Разбивает строку на слова, используя пробел как разделитель
        /// </summary>
        /// <returns>Массив слов из строки</returns>
        private string[] SplitLine(string line)
        {
            return line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .Where(w => !string.IsNullOrEmpty(w))
                .ToArray();
        }

        /// <summary>
        /// Строит индекс слов для быстрого поиска
        /// </summary>
        /// <returns>Словарь индекса для быстрого поиска слов</returns>
        private Dictionary<string, (int, int)> BuildIndex()
        {
            var index = new Dictionary<string, (int, int)>();

            // Индексируем слова из файла A (идентификатор файла: 0)
            for (int i = 0; i < synonymsA.Count; i++)
            {
                foreach (var word in synonymsA[i])
                {
                    string normalizedWord = word.ToLowerInvariant();
                    if (!index.ContainsKey(normalizedWord))
                    {
                        index[normalizedWord] = (i, 0);
                    }
                }
            }

            // Индексируем слова из файла B (идентификатор файла: 1)
            for (int i = 0; i < synonymsB.Count; i++)
            {
                foreach (var word in synonymsB[i])
                {
                    string normalizedWord = word.ToLowerInvariant();
                    if (!index.ContainsKey(normalizedWord))
                    {
                        index[normalizedWord] = (i, 1);
                    }
                }
            }

            return index;
        }

        /// <summary>
        /// Строит список всех уникальных слов
        /// </summary>
        private void BuildAllWordsList()
        {
            allWords.Clear();

            // Добавляем все слова из файла A
            foreach (var wordArray in synonymsA)
            {
                foreach (var word in wordArray)
                {
                    string normalizedWord = word.ToLowerInvariant();
                    if (!allWords.Contains(normalizedWord))
                    {
                        allWords.Add(normalizedWord);
                    }
                }
            }

            // Добавляем все слова из файла B
            foreach (var wordArray in synonymsB)
            {
                foreach (var word in wordArray)
                {
                    string normalizedWord = word.ToLowerInvariant();
                    if (!allWords.Contains(normalizedWord))
                    {
                        allWords.Add(normalizedWord);
                    }
                }
            }
        }

        #endregion
    }
}