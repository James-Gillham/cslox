namespace cslox.src;

public class Scanner(string Source)
{
    private static readonly Dictionary<string, TokenType> _keywords;
    private List<Token> _tokens = [];
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    static Scanner()
    {
        _keywords = new Dictionary<string, TokenType>()
        {
            { "and", TokenType.AND },
            { "class", TokenType.CLASS },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "fun", TokenType.FUN },
            { "if", TokenType.IF },
            { "nil", TokenType.NIL },
            { "or", TokenType.OR },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super", TokenType.SUPER },
            { "this", TokenType.THIS },
            { "true", TokenType.TRUE },
            { "var", TokenType.VAR },
            { "while", TokenType.WHILE }
        };
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            // We are at the beginning of the next lexeme
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '+': AddToken(TokenType.PLUS); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '*': AddToken(TokenType.STAR); break;
            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line
                    while (Peek() != '\n' && !IsAtEnd())
                    {
                        Advance();
                    }
                }
                else if (Match('*'))
                {
                    BlockComment();
                }
                else
                {
                    AddToken(TokenType.SLASH);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace
                break;
            case '\n':
                _line++;
                break;
            case '"': String(); break;
            
            default:
                if (IsDigit(c))
                {
                    Number();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    Lox.Error(_line, "Unexpected character.");
                }
                break;
        }
    }

    private void BlockComment()
    {
        while (!IsAtEnd())
        {
            // If we are terminating the block comment
            if (Peek() == '*' && PeekNext() == '/')
            {
                Advance();
                Advance();
                return;
            }
            if (Peek() == '\n')
            {
                _line++;
            }

            Advance();
        }

        Lox.Error(_line, "Unterminated block comment.");
        return;
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek()))
        {
            Advance();
        }

        var text = Source[_start.._current];
        if (_keywords.TryGetValue(text, out var type))
        {
            // Reserved keyword
            AddToken(type);
            return;
        }
        AddToken(TokenType.IDENTIFIER);
    }

    private void Number()
    {
        while (IsDigit(Peek()))
        {
            Advance();
        }

        // Look for fractional part
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // Consume the '.'
            Advance();

            while (IsDigit(Peek()))
            {
                Advance();
            }
        }

        AddToken(TokenType.NUMBER, Convert.ToDouble(Source[_start.._current]));
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
            {
                _line++;
            }
            Advance();
        }

        if (IsAtEnd())
        {
            Lox.Error(_line, "Unterminated string.");
            return;
        }

        // The closing "
        Advance();

        // Trim the surrounding quotes.
        var value = Source[(_start + 1)..(_current - 1)];
        AddToken(TokenType.STRING, value);
    }

    private bool Match(char expected)
    {
        if (IsAtEnd())
        {
            return false;
        }
        if (Source[_current] != expected)
        {
            return false;
        }

        _current++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd())
        {
            return '\0';
        }

        return Source[_current];
    }

    private char PeekNext()
    {
        if (_current + 1 >= Source.Length)
        {
            return '\0';
        }
        return Source[_current + 1];
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               (c == '_');
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    private bool IsAtEnd()
    {
        return _current >= Source.Length;
    }

    private char Advance() => Source[_current++];

    private void AddToken(TokenType type) => AddToken(type, null);

    private void AddToken(TokenType type, object? literal)
    {
        var text = Source[_start.._current];
        _tokens.Add(new Token(type, text, literal, _line));
    }
}