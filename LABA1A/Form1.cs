using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LABA1A
{
    public partial class Form1 : Form
    {
        private string currentFilePath = string.Empty;
        private bool isTextModified = false;
        private SplitContainer splitContainer;

        // Ключевые слова для подсветки синтаксиса (дополнительное задание)
        private readonly List<string> keywords = new List<string>
        {
            "if", "else", "while", "for", "foreach", "switch", "case",
            "break", "continue", "return", "int", "string", "float",
            "double", "bool", "char", "void", "class", "public",
            "private", "protected", "static", "void", "namespace", "using"
        };

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            this.Text = "Текстовый редактор - Лабораторная работа 1";
            UpdateTitle();

            // Подписываемся на события
            this.FormClosing += Form1_FormClosing;
        }

        private void InitializeCustomComponents()
        {
            // Очищаем форму от стандартных элементов управления
            this.Controls.Clear();

            // Создаем SplitContainer для изменения размеров областей
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            // Настраиваем RichTextBox для редактирования
            //richTextBox1 = new RichTextBox
            //{
            //    Dock = DockStyle.Fill,
            //    WordWrap = false,
            //    Font = new Font("Consolas", 10),
            //    ScrollBars = RichTextBoxScrollBars.Both
            //};
            //richTextBox1.TextChanged += RichTextBox1_TextChanged;

            // Настраиваем DataGridView для вывода результатов
            dataGridView1 = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            // Добавляем колонки
            dataGridView1.Columns.Add("Line", "Строка");
            dataGridView1.Columns.Add("Position", "Позиция");
            dataGridView1.Columns.Add("Message", "Сообщение");
            dataGridView1.Columns["Line"].Width = 60;
            dataGridView1.Columns["Position"].Width = 60;

            // Добавляем обработчик клика
            dataGridView1.CellClick += DataGridView1_CellClick;

            // Добавляем элементы в SplitContainer
            splitContainer.Panel1.Controls.Add(richTextBox1);
            splitContainer.Panel2.Controls.Add(dataGridView1);

            // Устанавливаем начальное соотношение размеров (70%/30%)
            splitContainer.SplitterDistance = (int)(this.ClientSize.Height * 0.7);

            // Добавляем SplitContainer на форму
            this.Controls.Add(splitContainer);

            // Добавляем menuStrip и toolStrip обратно
            this.Controls.Add(menuStrip1);
            this.Controls.Add(toolStrip1);

            // Устанавливаем правильный порядок
            menuStrip1.BringToFront();
            toolStrip1.BringToFront();

            // Подключаем обработчики событий для меню
            SubscribeMenuEvents();
        }

        private void SubscribeMenuEvents()
        {
            // Файл
            новыйФаилToolStripMenuItem.Click += NewFile_Click;
            открытьToolStripMenuItem.Click += OpenFile_Click;
            сохранитьToolStripMenuItem.Click += SaveFile_Click;
            сохранитьКакToolStripMenuItem.Click += SaveAsFile_Click;
            выходToolStripMenuItem.Click += Exit_Click;

            // Правка
            отменитьToolStripMenuItem.Click += Undo_Click;
            повторитьToolStripMenuItem.Click += Redo_Click;
            вырезатьToolStripMenuItem.Click += Cut_Click;
            копироватьToolStripMenuItem.Click += Copy_Click;
            вставитьToolStripMenuItem.Click += Paste_Click;
            удалитьToolStripMenuItem.Click += Delete_Click;
            выделитьВсеToolStripMenuItem.Click += SelectAll_Click;

            // Текст (пока просто показываем сообщения)
            постановкаЗадачиToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Постановка задачи", "Здесь будет постановка задачи");
            грамматикаToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Грамматика", "Здесь будет описание грамматики");
            классификацияГрамматикиToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Классификация грамматики", "Здесь будет классификация грамматики");
            методАнализаToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Метод анализа", "Здесь будет описание метода анализа");
            тексToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Тестовый пример", "Здесь будет тестовый пример");
            списокЛитературыToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Список литературы", "Здесь будет список литературы");
            исходныйКодПрограммыToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Исходный код программы", "Здесь будет исходный код");

            // Пуск
            пускToolStripMenuItem.Click += StartAnalysis_Click;

            // Справка
            вызовСправкиToolStripMenuItem.Click += Help_Click;
            оПрограммеToolStripMenuItem.Click += About_Click;

            // Панель инструментов
            toolStripButton1.Click += NewFile_Click;      // Создать
            toolStripButton2.Click += OpenFile_Click;      // Открыть
            toolStripButton3.Click += SaveFile_Click;      // Сохранить
            toolStripButton4.Click += Undo_Click;          // Отменить
            toolStripButton5.Click += Redo_Click;          // Повторить
            toolStripButton6.Click += Copy_Click;          // Копировать
            toolStripButton7.Click += Cut_Click;           // Вырезать
            toolStripButton8.Click += Paste_Click;         // Вставить
            toolStripButton9.Click += StartAnalysis_Click; // Пуск
            toolStripButton10.Click += Help_Click;         // Вызов справки
            toolStripButton11.Click += About_Click;        // О программе
        }

        private void ShowInfoWindow(string title, string content)
        {
            Form infoForm = new Form
            {
                Text = title,
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                Font = new Font("Consolas", 10)
            };

            RichTextBox textBox = new RichTextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Text = content,
                ReadOnly = true,
                Font = new Font("Consolas", 10),
                BackColor = Color.White
            };

            //  кнопка "Закрыть"
            Button closeButton = new Button
            {
                Text = "Закрыть",
                Dock = DockStyle.Bottom,
                Height = 30,
                FlatStyle = FlatStyle.Flat
            };
            closeButton.Click += (s, args) => infoForm.Close();

            infoForm.Controls.Add(textBox);
            infoForm.Controls.Add(closeButton);
            infoForm.ShowDialog();
        }

        private void UpdateTitle()
        {
            string fileName = string.IsNullOrEmpty(currentFilePath) ? "Безымянный" : Path.GetFileName(currentFilePath);
            this.Text = $"{fileName}{(isTextModified ? "*" : "")} - Текстовый редактор";
        }

        private bool PromptSaveChanges()
        {
            if (!isTextModified) return true;

            DialogResult result = MessageBox.Show(
                "Сохранить изменения в файле?",
                "Сохранение",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                return SaveFile();
            }
            else if (result == DialogResult.Cancel)
            {
                return false;
            }
            return true;
        }

        private bool SaveFile()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                return SaveAsFile();
            }
            else
            {
                try
                {
                    File.WriteAllText(currentFilePath, richTextBox1.Text);
                    isTextModified = false;
                    UpdateTitle();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private bool SaveAsFile()
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.RestoreDirectory = true;

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = saveDialog.FileName;
                    return SaveFile();
                }
            }
            return false;
        }

        // Обработчики событий меню "Файл"
        private void NewFile_Click(object sender, EventArgs e)
        {
            if (PromptSaveChanges())
            {
                richTextBox1.Clear();
                currentFilePath = string.Empty;
                isTextModified = false;
                UpdateTitle();
            }
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            if (!PromptSaveChanges()) return;

            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                openDialog.FilterIndex = 1;
                openDialog.RestoreDirectory = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        richTextBox1.Text = File.ReadAllText(openDialog.FileName);
                        currentFilePath = openDialog.FileName;
                        isTextModified = false;
                        UpdateTitle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии файла: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveFile_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void SaveAsFile_Click(object sender, EventArgs e)
        {
            SaveAsFile();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            if (PromptSaveChanges())
            {
                Application.Exit();
            }
        }

        // Обработчики событий меню "Правка"
        private void Undo_Click(object sender, EventArgs e)
        {
            if (richTextBox1.CanUndo)
            {
                richTextBox1.Undo();
            }
        }

        private void Redo_Click(object sender, EventArgs e)
        {
            if (richTextBox1.CanRedo)
            {
                richTextBox1.Redo();
            }
        }

        private void Cut_Click(object sender, EventArgs e)
        {
            if (richTextBox1.SelectedText.Length > 0)
            {
                richTextBox1.Cut();
            }
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            if (richTextBox1.SelectedText.Length > 0)
            {
                richTextBox1.Copy();
            }
        }

        private void Paste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                richTextBox1.Paste();
            }
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (richTextBox1.SelectedText.Length > 0)
            {
                int selectionStart = richTextBox1.SelectionStart;
                int selectionLength = richTextBox1.SelectionLength;
                richTextBox1.Text = richTextBox1.Text.Remove(selectionStart, selectionLength);
                richTextBox1.SelectionStart = selectionStart;
            }
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }

        // Обработчик для кнопки "Пуск"
        // Обработчик для кнопки "Пуск"
        // Обработчик для кнопки "Пуск"
        private void StartAnalysis_Click(object sender, EventArgs e)
        {
            // Очищаем предыдущие результаты
            dataGridView1.Rows.Clear();

            // Получаем текст из редактора
            string text = richTextBox1.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                // Не добавляем никаких сообщений для пустого текста
                return;
            }

            // Анализируем текст
            AnalyzeText(text);

            // Подсветка синтаксиса (дополнительное задание)
            HighlightSyntax();
        }

        private void AnalyzeText(string text)
        {
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int lineNumber = 1;
            int openBrackets = 0;
            int closeBrackets = 0;

            // Сначала посчитаем все скобки в тексте
            foreach (char c in text)
            {
                if (c == '{') openBrackets++;
                if (c == '}') closeBrackets++;
            }

            foreach (string line in lines)
            {
                // Проверка на наличие точки с запятой в конце строки
                // Игнорируем строки с открывающими/закрывающими скобками
                string trimmedLine = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedLine) &&
                    !trimmedLine.StartsWith("//") &&
                    !trimmedLine.EndsWith("{") &&
                    !trimmedLine.EndsWith("}") &&
                    !trimmedLine.EndsWith(";"))
                {
                    // Проверяем, что это не строка с if, for, while без точки с запятой
                    if (!trimmedLine.StartsWith("if") &&
                        !trimmedLine.StartsWith("for") &&
                        !trimmedLine.StartsWith("while") &&
                        !trimmedLine.StartsWith("else"))
                    {
                        AddResult(lineNumber.ToString(), (line.Length + 1).ToString(),
                                 "Ошибка: Отсутствует точка с запятой ';' в конце строки");
                    }
                }

                lineNumber++;
            }

            // Проверка на несбалансированные скобки (один раз для всего текста)
            if (openBrackets > closeBrackets)
            {
                AddResult("1", "1",
                         "Предупреждение: Несбалансированные скобки - больше открывающих '{'");
            }
            else if (closeBrackets > openBrackets)
            {
                AddResult("1", "1",
                         "Предупреждение: Несбалансированные скобки - больше закрывающих '}'");
            }
        }

        private void AddResult(string line, string position, string message)
        {
            dataGridView1.Rows.Add(line, position, message);
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    string lineStr = dataGridView1.Rows[e.RowIndex].Cells["Line"].Value?.ToString();
                    string posStr = dataGridView1.Rows[e.RowIndex].Cells["Position"].Value?.ToString();

                    if (!string.IsNullOrEmpty(lineStr) && int.TryParse(lineStr, out int line))
                    {
                        // Переходим к указанной строке
                        int charIndex = 0;
                        string[] lines = richTextBox1.Lines;

                        for (int i = 0; i < line - 1 && i < lines.Length; i++)
                        {
                            charIndex += lines[i].Length + Environment.NewLine.Length;
                        }

                        if (!string.IsNullOrEmpty(posStr) && int.TryParse(posStr, out int position))
                        {
                            // Корректируем позицию (не выходим за границы строки)
                            if (line - 1 < lines.Length)
                            {
                                position = Math.Min(position, lines[line - 1].Length);
                            }
                            charIndex += Math.Max(0, position - 1);
                        }

                        if (charIndex >= 0 && charIndex < richTextBox1.TextLength)
                        {
                            richTextBox1.Focus();

                            // СБРАСЫВАЕМ ЦВЕТ ВСЕГО ТЕКСТА ПЕРЕД НОВЫМ ВЫДЕЛЕНИЕМ
                            int originalSelectionStart = richTextBox1.SelectionStart;
                            int originalSelectionLength = richTextBox1.SelectionLength;

                            richTextBox1.SelectAll();
                            richTextBox1.SelectionColor = Color.Black;

                            // Теперь выделяем ошибку
                            richTextBox1.Select(charIndex, 0);

                            // Выделяем слово или символ, где ошибка
                            int endOfLine = richTextBox1.Text.IndexOf('\n', charIndex);
                            if (endOfLine == -1) endOfLine = richTextBox1.TextLength;

                            // Ищем конец текущего слова
                            int endOfWord = charIndex;
                            while (endOfWord < endOfLine && !char.IsWhiteSpace(richTextBox1.Text[endOfWord]))
                            {
                                endOfWord++;
                            }

                            int length = Math.Max(1, endOfWord - charIndex);
                            richTextBox1.SelectionLength = length;
                            richTextBox1.SelectionColor = Color.Red;

                            richTextBox1.ScrollToCaret();

                            // Восстанавливаем выделение после подсветки синтаксиса
                            // Но оставляем красное выделение для видимости ошибки
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Игнорируем ошибки перехода
                }
            }
        }

        // Добавим обработчик TextChanged для сброса цвета при изменении текста
        private void RichTextBox1_TextChanged1(object sender, EventArgs e)
        {
            isTextModified = true;
            UpdateTitle();

            // Сбрасываем цвет текста при изменении
            // Но делаем это с задержкой, чтобы не мешать вводу
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 500;
            timer.Tick += (s, args) =>
            {
                // Возвращаем черный цвет для всего текста
                int selectionStart = richTextBox1.SelectionStart;
                int selectionLength = richTextBox1.SelectionLength;

                richTextBox1.SelectAll();
                richTextBox1.SelectionColor = Color.Black;

                richTextBox1.Select(selectionStart, selectionLength);

                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        // Подсветка синтаксиса (дополнительное задание)
        // Подсветка синтаксиса (дополнительное задание)
        private void HighlightSyntax()
        {
            if (richTextBox1.TextLength == 0) return;

            int selectionStart = richTextBox1.SelectionStart;
            int selectionLength = richTextBox1.SelectionLength;

            // Сохраняем оригинальный текст
            string text = richTextBox1.Text;

            // Временно отключаем обработчик TextChanged
            richTextBox1.TextChanged -= RichTextBox1_TextChanged;

            // Сбрасываем форматирование ТОЛЬКО если нет красного выделения ошибки
            bool hasRedSelection = false;
            for (int i = 0; i < richTextBox1.TextLength; i++)
            {
                richTextBox1.Select(i, 1);
                if (richTextBox1.SelectionColor == Color.Red)
                {
                    hasRedSelection = true;
                    break;
                }
            }

            if (!hasRedSelection)
            {
                richTextBox1.SelectAll();
                richTextBox1.SelectionColor = Color.Black;
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
            }

            // Разбиваем на слова и выделяем ключевые слова
            string[] words = text.Split(new[] { ' ', '\n', '\r', '\t', '(', ')', '{', '}', ';', ',', '.', '=', '+', '-', '*', '/' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                // Проверяем, является ли слово ключевым
                if (keywords.Contains(word.ToLower()))
                {
                    // Находим все вхождения слова
                    int index = 0;
                    while ((index = text.IndexOf(word, index, StringComparison.Ordinal)) != -1)
                    {
                        // Проверяем, что это отдельное слово
                        bool isWholeWord = true;

                        if (index > 0)
                        {
                            char prevChar = text[index - 1];
                            if (char.IsLetterOrDigit(prevChar) || prevChar == '_')
                                isWholeWord = false;
                        }

                        int endIndex = index + word.Length;
                        if (endIndex < text.Length)
                        {
                            char nextChar = text[endIndex];
                            if (char.IsLetterOrDigit(nextChar) || nextChar == '_')
                                isWholeWord = false;
                        }

                        // Проверяем, не красное ли это выделение
                        richTextBox1.Select(index, word.Length);
                        if (isWholeWord && richTextBox1.SelectionColor != Color.Red)
                        {
                            richTextBox1.SelectionColor = Color.Blue;
                            richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                        }

                        index += word.Length;
                    }
                }
            }

            // Возвращаем выделение на место
            richTextBox1.Select(selectionStart, selectionLength);

            // Включаем обработчик обратно
            richTextBox1.TextChanged += RichTextBox1_TextChanged;
        }

        // Обработчики событий меню "Справка"
        private void Help_Click(object sender, EventArgs e)
        {
            string helpText =
                "Справка по программе\r\n" +
                "=====================\r\n\r\n" +
                "Меню 'Файл':\r\n" +
                "  • Создать - создает новый файл\r\n" +
                "  • Открыть - открывает существующий файл\r\n" +
                "  • Сохранить - сохраняет текущий файл\r\n" +
                "  • Сохранить как - сохраняет файл под новым именем\r\n" +
                "  • Выход - выход из программы\r\n\r\n" +
                "Меню 'Правка':\r\n" +
                "  • Отменить - отменяет последнее действие\r\n" +
                "  • Повторить - повторяет отмененное действие\r\n" +
                "  • Вырезать - вырезает выделенный текст\r\n" +
                "  • Копировать - копирует выделенный текст\r\n" +
                "  • Вставить - вставляет текст из буфера обмена\r\n" +
                "  • Удалить - удаляет выделенный текст\r\n" +
                "  • Выделить все - выделяет весь текст\r\n\r\n" +
                "Меню 'Текст':\r\n" +
                "  • Постановка задачи\r\n" +
                "  • Грамматика\r\n" +
                "  • Классификация грамматики\r\n" +
                "  • Метод анализа\r\n" +
                "  • Тестовый пример\r\n" +
                "  • Список литературы\r\n" +
                "  • Исходный код программы\r\n\r\n" +
                "Меню 'Пуск' - запускает анализатор текста\r\n\r\n" +
                "Дополнительные возможности:\r\n" +
                "  • Подсветка синтаксиса ключевых слов\r\n" +
                "  • Переход к ошибке при клике в таблице результатов\r\n" +
                "  • Изменение размеров областей редактирования и вывода";

            ShowInfoWindow("Справка", helpText);
        }

        private void About_Click(object sender, EventArgs e)
        {
            string aboutText =
                "Текстовый редактор\r\n" +
                "==================\r\n\r\n" +
                "Версия 1.0\r\n\r\n" +
                "Лабораторная работа №1\r\n" +
                "Разработчик: Ковалев Егор\r\n\r\n" +
                "Функции:\r\n" +
                "✓ Работа с файлами\r\n" +
                "✓ Редактирование текста\r\n" +
                "✓ Подсветка синтаксиса\r\n" +
                "✓ Анализ текста\r\n\r\n" +
                "© 2026";

            MessageBox.Show(aboutText, "О программе",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {
            isTextModified = true;
            UpdateTitle();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!PromptSaveChanges())
            {
                e.Cancel = true;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (splitContainer != null && this.WindowState != FormWindowState.Minimized)
            {
                try
                {
                    // Обновляем положение разделителя при изменении размера окна
                    splitContainer.SplitterDistance = (int)(this.ClientSize.Height * 0.7);
                }
                catch
                {
                    // Игнорируем ошибки при изменении размера
                }
            }
        }
    }
}