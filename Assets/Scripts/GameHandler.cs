using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public int width = 4;
    public int height = 4;
    public int mineCount = 2;

    private Board board;
    private Cell[,] cells;
    private bool gameover;

    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        board = GetComponent<Board>();
    }
    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        cells = new Cell[width, height];
        gameover = false;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        board.Draw(cells);
    }

    private void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                cells[x, y] = cell;
            }
        }
    }
    private void GenerateMines()
    {
        for (int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            while (cells[x, y].type == Cell.Type.Mine)
            {
                x++;
                if (x >= width)
                {
                    x = 0;
                    y++;
                    if (y >= height)
                    {
                        y = 0;
                    }
                }
            }

            cells[x, y].type = Cell.Type.Mine;
        }
    }
    private void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = cells[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    continue;
                }
                cell.number = CountMines(x, y);
                if (cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                cells[x, y] = cell;
            }
        }
    }

    private int CountMines(int x, int y)
    {
        int count = 0;

        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                if (offsetX == 0 && offsetY == 0)
                {
                    continue;
                }

                int cellX = offsetX + x;
                int cellY = offsetY + y;
                if (GetCell(cellX, cellY).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private Cell GetCell(int cellX, int cellY)
    {
        if (IsInBounds(cellX, cellY))
        {
            return cells[cellX, cellY];
        }
        else
        {
            return new Cell();
        }
    }

    private bool IsInBounds(int cellX, int cellY)
    {
        return cellX >= 0 && cellX < width && cellY >= 0 && cellY < height;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartNewGame();
        }
        else if (!gameover)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Reveal();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Flag();
            }
        }
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellpos = board.tilemap.WorldToCell(worldPosition);

        Cell cell = GetCell(cellpos.x, cellpos.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }

        cell.flagged = !cell.flagged;
        cells[cellpos.x, cellpos.y] = cell;
        board.Draw(cells);
    }

    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellpos = board.tilemap.WorldToCell(worldPosition);

        Cell cell = GetCell(cellpos.x, cellpos.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }
        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                cell.revealed = true;
                cells[cellpos.x, cellpos.y] = cell;
                CheckWinCondition();
                break;
        }

        board.Draw(cells);
    }


    private void Flood(Cell cell)
    {
        if (cell.revealed)
        {
            return;
        }
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid)
        {
            return;
        }
        cell.revealed = true;
        cells[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
        }
    }

    private void Explode(Cell cell)
    {
        gameover = true;

        cell.exploded = true;
        cell.revealed = true;
        cells[cell.position.x, cell.position.y] = cell;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cell = cells[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    cells[x, y] = cell;
                }
            }
        }
    }
    private void CheckWinCondition()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = cells[x, y];

                if (cell.type != Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }
            }
        }

        gameover = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = cells[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    cells[x, y] = cell;
                }
            }
        }
    }
}