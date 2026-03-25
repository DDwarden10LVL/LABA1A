using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace LABA1A
{
    public partial class Form1 : Form
    {
        private string currentFilePath = string.Empty;
        private bool isTextModified = false;
        private SplitContainer splitContainer;
        private DataGridView dataGridViewErrors;
        private Label lblErrorCount;

        private enum TokenType
        {
            KeywordPrivate = 1,
            KeywordPrivateProtected = 2,
            KeywordProtected = 3,
            KeywordInternal = 4,
            KeywordInternalProtected = 5,
            KeywordPublic = 6,
            KeywordAbstract = 7,
            KeywordVoid = 8,
            KeywordInt = 9,
            KeywordBool = 10,
            KeywordChar = 11,
            KeywordString = 12,
            Whitespace = 13,
            OpenParen = 14,
            CloseParen = 15,
            Comma = 16,
            Semicolon = 17,
            Identifier = 18,
            Error = -1
        }

        private class Token
        {
            public int Code { get; set; }
            public string Type { get; set; }
            public string Lexeme { get; set; }
            public int Line { get; set; }
            public int StartPos { get; set; }
            public int EndPos { get; set; }
        }

        public class SyntaxError
        {
            public string Fragment { get; set; }
            public int Line { get; set; }
            public int Position { get; set; }
            public string Description { get; set; }
        }

        private class Scanner
        {
            private HashSet<string> keywords = new HashSet<string>
            {
                "private", "protected", "internal", "public", "abstract",
                "void", "int", "bool", "char", "string"
            };

            private enum State
            {
                Start,
                KeywordOrIdentifier,
                Delimiter,
                Operator,
                Error,
                Finish
            }

            private string text;
            private int position;
            private int line;
            private int lineStartPos;
            private List<Token> tokens;
            private StringBuilder lexemeBuffer;
            private int lexemeStartPos;
            private int lexemeStartLine;

            public List<Token> Analyze(string inputText)
            {
                text = inputText;
                position = 0;
                line = 1;
                lineStartPos = 0;
                tokens = new List<Token>();
                lexemeBuffer = new StringBuilder();

                State currentState = State.Start;

                while (currentState != State.Finish)
                {
                    switch (currentState)
                    {
                        case State.Start:
                            currentState = ProcessStart();
                            break;
                        case State.KeywordOrIdentifier:
                            currentState = ProcessKeywordOrIdentifier();
                            break;
                        case State.Delimiter:
                            currentState = ProcessDelimiter();
                            break;
                        case State.Operator:
                            currentState = ProcessOperator();
                            break;
                        case State.Error:
                            currentState = ProcessError();
                            break;
                    }
                }

                return tokens;
            }

            private State ProcessStart()
            {
                if (position >= text.Length)
                {
                    return State.Finish;
                }

                char currentChar = text[position];

                lexemeBuffer.Clear();
                lexemeStartPos = position - lineStartPos;
                lexemeStartLine = line;

                if (char.IsLetter(currentChar) || currentChar == '_')
                {
                    lexemeBuffer.Append(currentChar);
                    position++;
                    return State.KeywordOrIdentifier;
                }
                else if (currentChar == ' ')
                {
                    lexemeBuffer.Append(currentChar);
                    position++;
                    return State.Delimiter;
                }
                else if (currentChar == '(' || currentChar == ')' || currentChar == ',' || currentChar == ';')
                {
                    lexemeBuffer.Append(currentChar);
                    position++;
                    return State.Operator;
                }
                else if (currentChar == '\n')
                {
                    line++;
                    lineStartPos = position + 1;
                    position++;
                    return State.Start;
                }
                else if (currentChar == '\r')
                {
                    position++;
                    return State.Start;
                }
                else
                {
                    lexemeBuffer.Append(currentChar);
                    position++;
                    return State.Error;
                }
            }

            private State ProcessKeywordOrIdentifier()
            {
                if (position >= text.Length)
                {
                    FinalizeKeywordOrIdentifier();
                    return State.Finish;
                }

                char currentChar = text[position];

                if (char.IsLetterOrDigit(currentChar) || currentChar == '_')
                {
                    lexemeBuffer.Append(currentChar);
                    position++;
                    return State.KeywordOrIdentifier;
                }
                else
                {
                    FinalizeKeywordOrIdentifier();
                    return State.Start;
                }
            }

            private State ProcessDelimiter()
            {
                FinalizeDelimiter();
                return State.Start;
            }

            private State ProcessOperator()
            {
                FinalizeOperator();
                return State.Start;
            }

            private State ProcessError()
            {
                FinalizeError();
                return State.Start;
            }

            private void FinalizeKeywordOrIdentifier()
            {
                string lexeme = lexemeBuffer.ToString();
                Token token = new Token
                {
                    Lexeme = lexeme,
                    Line = lexemeStartLine,
                    StartPos = lexemeStartPos,
                    EndPos = lexemeStartPos + lexeme.Length - 1
                };

                if (keywords.Contains(lexeme))
                {
                    switch (lexeme)
                    {
                        case "private":
                            token.Code = (int)TokenType.KeywordPrivate;
                            token.Type = "ключевое слово private";
                            break;
                        case "protected":
                            token.Code = (int)TokenType.KeywordProtected;
                            token.Type = "ключевое слово protected";
                            break;
                        case "internal":
                            token.Code = (int)TokenType.KeywordInternal;
                            token.Type = "ключевое слово internal";
                            break;
                        case "public":
                            token.Code = (int)TokenType.KeywordPublic;
                            token.Type = "ключевое слово public";
                            break;
                        case "abstract":
                            token.Code = (int)TokenType.KeywordAbstract;
                            token.Type = "ключевое слово abstract";
                            break;
                        case "void":
                            token.Code = (int)TokenType.KeywordVoid;
                            token.Type = "ключевое слово void";
                            break;
                        case "int":
                            token.Code = (int)TokenType.KeywordInt;
                            token.Type = "ключевое слово int";
                            break;
                        case "bool":
                            token.Code = (int)TokenType.KeywordBool;
                            token.Type = "ключевое слово bool";
                            break;
                        case "char":
                            token.Code = (int)TokenType.KeywordChar;
                            token.Type = "ключевое слово char";
                            break;
                        case "string":
                            token.Code = (int)TokenType.KeywordString;
                            token.Type = "ключевое слово string";
                            break;
                    }
                }
                else
                {
                    token.Code = (int)TokenType.Identifier;
                    token.Type = "идентификатор";
                }

                tokens.Add(token);
            }

            private void FinalizeDelimiter()
            {
                string lexeme = lexemeBuffer.ToString();
                Token token = new Token
                {
                    Lexeme = lexeme,
                    Code = (int)TokenType.Whitespace,
                    Type = "разделитель (пробел)",
                    Line = lexemeStartLine,
                    StartPos = lexemeStartPos,
                    EndPos = lexemeStartPos + lexeme.Length - 1
                };

                tokens.Add(token);
            }

            private void FinalizeOperator()
            {
                string lexeme = lexemeBuffer.ToString();
                char op = lexeme[0];

                Token token = new Token
                {
                    Lexeme = lexeme,
                    Line = lexemeStartLine,
                    StartPos = lexemeStartPos,
                    EndPos = lexemeStartPos + lexeme.Length - 1
                };

                switch (op)
                {
                    case '(':
                        token.Code = (int)TokenType.OpenParen;
                        token.Type = "открывающая скобка";
                        break;
                    case ')':
                        token.Code = (int)TokenType.CloseParen;
                        token.Type = "закрывающая скобка";
                        break;
                    case ',':
                        token.Code = (int)TokenType.Comma;
                        token.Type = "разделитель (запятая)";
                        break;
                    case ';':
                        token.Code = (int)TokenType.Semicolon;
                        token.Type = "конец оператора";
                        break;
                }

                tokens.Add(token);
            }

            private void FinalizeError()
            {
                string lexeme = lexemeBuffer.ToString();
                Token token = new Token
                {
                    Lexeme = lexeme,
                    Code = (int)TokenType.Error,
                    Type = "НЕДОПУСТИМЫЙ СИМВОЛ",
                    Line = lexemeStartLine,
                    StartPos = lexemeStartPos,
                    EndPos = lexemeStartPos + lexeme.Length - 1
                };

                tokens.Add(token);
            }
        }

        private class Parser
        {
            private List<Token> tokens;
            private int currentPos;
            private Token currentToken;
            private List<SyntaxError> errors;
            private int currentLine;
            private int currentPosInLine;

            private HashSet<string> modyfSet = new HashSet<string>
            {
                "public", "internal", "protected"
            };

            private HashSet<string> tipSet = new HashSet<string>
            {
                "int", "string", "char", "void"
            };

            public List<SyntaxError> Parse(List<Token> tokens)
            {
                this.tokens = tokens;
                this.currentPos = 0;
                this.errors = new List<SyntaxError>();
                this.currentLine = 1;
                this.currentPosInLine = 0;

                if (tokens == null || tokens.Count == 0)
                {
                    return errors;
                }

                currentToken = tokens[0];
                SkipWhitespace();

                if (currentToken != null)
                {
                    Start();
                }

                return errors;
            }

            private void SkipWhitespace()
            {
                while (currentToken != null && currentToken.Code == (int)TokenType.Whitespace)
                {
                    NextToken();
                }
            }

            private void NextToken()
            {
                currentPos++;
                if (currentPos < tokens.Count)
                {
                    currentToken = tokens[currentPos];
                    if (currentToken != null)
                    {
                        currentLine = currentToken.Line;
                        currentPosInLine = currentToken.StartPos;
                    }
                }
                else
                {
                    currentToken = null;
                }
            }

            private void AddError(string description, Token token = null)
            {
                Token errToken = token ?? currentToken;
                errors.Add(new SyntaxError
                {
                    Fragment = errToken?.Lexeme ?? "конец строки",
                    Line = errToken?.Line ?? currentLine,
                    Position = errToken?.StartPos ?? currentPosInLine,
                    Description = description
                });
            }

            private bool CheckAndConsume(string expected, string errorMsg)
            {
                SkipWhitespace();

                if (currentToken != null && currentToken.Lexeme == expected)
                {
                    NextToken();
                    return true;
                }
                else
                {
                    AddError(errorMsg);
                    return false;
                }
            }

            private bool CheckAndConsumeType(HashSet<string> expectedSet, string errorMsg)
            {
                SkipWhitespace();

                if (currentToken != null && expectedSet.Contains(currentToken.Lexeme))
                {
                    NextToken();
                    return true;
                }
                else
                {
                    AddError(errorMsg);
                    return false;
                }
            }

            private bool CheckAndConsumeIdentifier(string errorMsg)
            {
                SkipWhitespace();

                if (currentToken != null && currentToken.Code == (int)TokenType.Identifier)
                {
                    NextToken();
                    return true;
                }
                else
                {
                    AddError(errorMsg);
                    return false;
                }
            }

            private void Start()
            {
                CheckAndConsumeType(modyfSet, "Ожидался модификатор доступа (public/internal/protected)");
                CheckAndConsume("abstract", "Ожидалось ключевое слово 'abstract'");
                CheckAndConsumeType(tipSet, "Ожидался тип возвращаемого значения (int/string/char/void)");
                CheckAndConsumeIdentifier("Ожидалось имя метода (идентификатор)");
                CheckAndConsume("(", "Ожидалась открывающая скобка '('");

                Params();

                CheckAndConsume(")", "Ожидалась закрывающая скобка ')'");
                CheckAndConsume(";", "Ожидалась ';' в конце объявления метода");

                SkipWhitespace();
                if (currentToken != null)
                {
                    AddError("Лишние символы после окончания объявления метода");
                }
            }

            private void Params()
            {
                SkipWhitespace();

                if (currentToken != null && currentToken.Lexeme == ")")
                {
                    return;
                }

                Param();

                SkipWhitespace();
                while (currentToken != null && currentToken.Lexeme == ",")
                {
                    NextToken();
                    Param();
                    SkipWhitespace();
                }
            }

            private void Param()
            {
                CheckAndConsumeType(tipSet, "Ожидался тип параметра (int/string/char/void)");
                CheckAndConsumeIdentifier("Ожидалось имя параметра (идентификатор)");
            }
        }

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
            this.Text = "Текстовый редактор - Лабораторная работа 3";
            UpdateTitle();
            this.FormClosing += Form1_FormClosing;
        }

        private void InitializeCustomComponents()
        {
            this.Controls.Clear();

            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            richTextBox1.TextChanged += RichTextBox1_TextChanged;

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

            dataGridView1.Columns.Add("Code", "Условный код");
            dataGridView1.Columns.Add("Type", "Тип лексемы");
            dataGridView1.Columns.Add("Lexeme", "Лексема");
            dataGridView1.Columns.Add("Location", "Местоположение");
            dataGridView1.Columns["Code"].Width = 80;
            dataGridView1.Columns["Type"].Width = 150;
            dataGridView1.Columns["Lexeme"].Width = 150;
            dataGridView1.Columns["Location"].Width = 120;

            dataGridView1.CellClick += DataGridView1_CellClick;

            dataGridViewErrors = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            dataGridViewErrors.Columns.Add("Fragment", "Неверный фрагмент");
            dataGridViewErrors.Columns.Add("Location", "Местоположение");
            dataGridViewErrors.Columns.Add("Description", "Описание ошибки");
            dataGridViewErrors.Columns["Fragment"].Width = 120;
            dataGridViewErrors.Columns["Location"].Width = 120;
            dataGridViewErrors.Columns["Description"].Width = 250;

            dataGridViewErrors.CellClick += DataGridViewErrors_CellClick;

            SplitContainer verticalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = (int)(this.ClientSize.Width * 0.5)
            };

            verticalSplit.Panel1.Controls.Add(dataGridView1);
            verticalSplit.Panel2.Controls.Add(dataGridViewErrors);

            splitContainer.Panel1.Controls.Add(richTextBox1);
            splitContainer.Panel2.Controls.Add(verticalSplit);

            splitContainer.SplitterDistance = (int)(this.ClientSize.Height * 0.6);

            this.Controls.Add(splitContainer);
            this.Controls.Add(menuStrip1);
            this.Controls.Add(toolStrip1);

            menuStrip1.BringToFront();
            toolStrip1.BringToFront();

            lblErrorCount = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightGray,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblErrorCount);

            SubscribeMenuEvents();
        }

        private void SubscribeMenuEvents()
        {
            новыйФаилToolStripMenuItem.Click += NewFile_Click;
            открытьToolStripMenuItem.Click += OpenFile_Click;
            сохранитьToolStripMenuItem.Click += SaveFile_Click;
            сохранитьКакToolStripMenuItem.Click += SaveAsFile_Click;
            выходToolStripMenuItem.Click += Exit_Click;

            отменитьToolStripMenuItem.Click += Undo_Click;
            повторитьToolStripMenuItem.Click += Redo_Click;
            вырезатьToolStripMenuItem.Click += Cut_Click;
            копироватьToolStripMenuItem.Click += Copy_Click;
            вставитьToolStripMenuItem.Click += Paste_Click;
            удалитьToolStripMenuItem.Click += Delete_Click;
            выделитьВсеToolStripMenuItem.Click += SelectAll_Click;

            постановкаЗадачиToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Постановка задачи", "Здесь будет постановка задачи");
            грамматикаToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Грамматика", "Здесь будет описание грамматики");
            классификацияГрамматикиToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Классификация грамматики", "Здесь будет классификация грамматики");
            методАнализаToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Метод анализа", "Здесь будет описание метода анализа");
            тексToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Тестовый пример", "Здесь будет тестовый пример");
            списокЛитературыToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Список литературы", "Здесь будет список литературы");
            исходныйКодПрограммыToolStripMenuItem.Click += (s, e) => ShowInfoWindow("Исходный код программы", "Здесь будет исходный код");

            пускToolStripMenuItem.Click += StartAnalysis_Click;

            вызовСправкиToolStripMenuItem.Click += Help_Click;
            оПрограммеToolStripMenuItem.Click += About_Click;

            toolStripButton1.Click += NewFile_Click;
            toolStripButton2.Click += OpenFile_Click;
            toolStripButton3.Click += SaveFile_Click;
            toolStripButton4.Click += Undo_Click;
            toolStripButton5.Click += Redo_Click;
            toolStripButton6.Click += Copy_Click;
            toolStripButton7.Click += Cut_Click;
            toolStripButton8.Click += Paste_Click;
            toolStripButton9.Click += StartAnalysis_Click;
            toolStripButton10.Click += Help_Click;
            toolStripButton11.Click += About_Click;
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

        private void StartAnalysis_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridViewErrors.Rows.Clear();

            string text = richTextBox1.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                lblErrorCount.Text = "Введите текст для анализа";
                lblErrorCount.ForeColor = Color.Orange;
                return;
            }

            Scanner scanner = new Scanner();
            List<Token> tokens = scanner.Analyze(text);

            foreach (var token in tokens)
            {
                int rowIndex = dataGridView1.Rows.Add(
                    token.Code,
                    token.Type,
                    token.Lexeme,
                    $"строка {token.Line}, {token.StartPos}-{token.EndPos}"
                );

                if (token.Code == -1)
                {
                    dataGridView1.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                }
            }

            Parser parser = new Parser();
            List<SyntaxError> errors = parser.Parse(tokens);

            if (errors.Count == 0)
            {
                lblErrorCount.ForeColor = Color.Green;
                lblErrorCount.Text = "Синтаксических ошибок не обнаружено!";
            }
            else
            {
                lblErrorCount.ForeColor = Color.Red;
                lblErrorCount.Text = $"Общее количество ошибок: {errors.Count}";

                foreach (var error in errors)
                {
                    int rowIndex = dataGridViewErrors.Rows.Add(
                        error.Fragment,
                        $"строка {error.Line}, позиция {error.Position}",
                        error.Description
                    );
                    dataGridViewErrors.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                }
            }

            HighlightSyntax();
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    string location = dataGridView1.Rows[e.RowIndex].Cells["Location"].Value?.ToString();
                    if (string.IsNullOrEmpty(location)) return;

                    string[] parts = location.Replace("строка ", "").Split(',');
                    if (parts.Length != 2) return;

                    if (int.TryParse(parts[0], out int line))
                    {
                        string[] positions = parts[1].Split('-');
                        if (positions.Length == 2 && int.TryParse(positions[0], out int startPos))
                        {
                            int charIndex = 0;
                            string[] lines = richTextBox1.Lines;

                            for (int i = 0; i < line - 1 && i < lines.Length; i++)
                            {
                                charIndex += lines[i].Length + Environment.NewLine.Length;
                            }

                            charIndex += startPos;

                            if (charIndex >= 0 && charIndex < richTextBox1.TextLength)
                            {
                                richTextBox1.Focus();
                                richTextBox1.Select(charIndex, 1);
                                richTextBox1.ScrollToCaret();
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void DataGridViewErrors_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    string location = dataGridViewErrors.Rows[e.RowIndex].Cells["Location"].Value?.ToString();
                    if (string.IsNullOrEmpty(location)) return;

                    string[] parts = location.Replace("строка ", "").Replace("позиция ", "").Split(',');
                    if (parts.Length != 2) return;

                    if (int.TryParse(parts[0].Trim(), out int line) &&
                        int.TryParse(parts[1].Trim(), out int position))
                    {
                        int charIndex = 0;
                        string[] lines = richTextBox1.Lines;

                        for (int i = 0; i < line - 1 && i < lines.Length; i++)
                        {
                            charIndex += lines[i].Length + Environment.NewLine.Length;
                        }

                        charIndex += position;

                        if (charIndex >= 0 && charIndex < richTextBox1.TextLength)
                        {
                            richTextBox1.Focus();
                            richTextBox1.Select(charIndex, 1);
                            richTextBox1.ScrollToCaret();
                        }
                    }
                }
                catch { }
            }
        }

        private void HighlightSyntax()
        {
            if (richTextBox1.TextLength == 0) return;

            int selectionStart = richTextBox1.SelectionStart;
            int selectionLength = richTextBox1.SelectionLength;

            string text = richTextBox1.Text;

            richTextBox1.TextChanged -= RichTextBox1_TextChanged;

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

            string[] words = text.Split(new[] { ' ', '\n', '\r', '\t', '(', ')', '{', '}', ';', ',', '.', '=', '+', '-', '*', '/' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                if (keywords.Contains(word.ToLower()))
                {
                    int index = 0;
                    while ((index = text.IndexOf(word, index, StringComparison.Ordinal)) != -1)
                    {
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

            richTextBox1.Select(selectionStart, selectionLength);
            richTextBox1.TextChanged += RichTextBox1_TextChanged;
        }

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
                "Меню 'Пуск' - запускает лексический и синтаксический анализ текста\r\n\r\n" +
                "Дополнительные возможности:\r\n" +
                "  • Подсветка синтаксиса ключевых слов\r\n" +
                "  • Переход к ошибке при клике в таблице результатов\r\n" +
                "  • Изменение размеров областей редактирования и вывода\r\n" +
                "  • Вывод списка лексем и синтаксических ошибок\r\n" +
                "  • Навигация по ошибкам синтаксиса";

            ShowInfoWindow("Справка", helpText);
        }

        private void About_Click(object sender, EventArgs e)
        {
            string aboutText =
                "Текстовый редактор с лексическим и синтаксическим анализатором\r\n" +
                "==========================================================\r\n\r\n" +
                "Версия 2.0\r\n\r\n" +
                "Лабораторные работы №1, №2, №3\r\n" +
                "Разработчик: Ковалев Егор\r\n" +
                "Группа: АП-327\r\n\r\n" +
                "Функции:\r\n" +
                "✓ Работа с файлами\r\n" +
                "✓ Редактирование текста\r\n" +
                "✓ Подсветка синтаксиса\r\n" +
                "✓ Лексический анализ (сканер)\r\n" +
                "✓ Синтаксический анализ (парсер)\r\n" +
                "✓ Навигация по ошибкам\r\n\r\n" +
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
                    splitContainer.SplitterDistance = (int)(this.ClientSize.Height * 0.6);
                }
                catch
                {
                }
            }
        }
    }
}