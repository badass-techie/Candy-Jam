using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    List<Sprite> availableSprites = new List<Sprite>();
    private int numSprites = 12;
    public int numTiles = 9;
    [HideInInspector] public float tileWidth;
    private GameObject[,] tiles;
    [Range(0f, 1f)] public float tileScale = 0.667f;
    public GameObject gridBackground;
    
    public GameObject gameOverMenu;
    public Text movesText, scoreText, gameOverScoreText, gameOverReasonText, highScoreText;
 
    public int startingMoves = 50, numMoves; // 3
    public int NumMoves
    {
        get => numMoves;
        set
        {
            numMoves = value;
            movesText.text = $"MOVES: {numMoves.ToString()}/{startingMoves.ToString()}";
        }
    }
 
    private int score;
    public int Score
    {
        get => score;
        set
        {
            score = value;
            scoreText.text = $"SCORE: {score.ToString()}";
            gameOverScoreText.text = $"{score.ToString()}";
        }
    }

    public int numAnimatingTiles;
    
    // Start is called before the first frame update
    void Start()
    {
        Score = 0;
        NumMoves = startingMoves;
        
        //establish Tile width
        float cameraHeight = Camera.main.orthographicSize * 2;
        float cameraWidth = cameraHeight * Camera.main.aspect;
        float margin = cameraWidth / 8;
        float gridWidth = cameraWidth - margin;
        tileWidth = gridWidth / numTiles;
        
        transform.Find("translucent background").localScale = Vector3.one * (numTiles * tileWidth + tileWidth/2);    //resize translucent background
        
        //load available sprites into memory
        for (int i = 0; i < numSprites; ++i) 
            availableSprites.Add(Resources.Load<Sprite>($"Candy/{i}"));

        tiles = new GameObject[numTiles, numTiles];
        CreateTiles();    //create Tiles
        while (!AnyMovesLeft())    //as long as the board has no valid moves
        {
            foreach (Transform child in transform)
            {
                if(child.gameObject.CompareTag("Tile"))
                    Destroy(child.gameObject);    //remove existing tiles
            }
            CreateTiles();    //recreate Tiles
        }
    }
    
    void CreateTiles()
    {
        Vector3 startPos = transform.position - Vector3.one * (numTiles * tileWidth/2 - tileWidth/2);
        for (int row = 0; row < numTiles; row++)
        {
            for (int column = 0; column < numTiles; column++)
            {
                GameObject newTile = new GameObject($"{column},{row}");
                newTile.tag = "Tile";
                SpriteRenderer renderer = newTile.AddComponent<SpriteRenderer>();

                switch (SceneManager.GetActiveScene().name)
                {
                    case "Easy":
                        //make sure 3 adjacent sprites do not match
                        //increase chance of finding matches
                        do
                        {
                            int prob = Random.Range(0, 100);
                            if (prob < 33 && column >= 1 && row >= 1)
                                renderer.sprite = tiles[column - 1, row - 1].GetComponent<SpriteRenderer>().sprite;
                            else if (prob > 67 && row >= 1 && column < numTiles-1)
                                renderer.sprite = tiles[column + 1, row - 1].GetComponent<SpriteRenderer>().sprite;
                            else
                                renderer.sprite = availableSprites[Random.Range(0, availableSprites.Count)];
                        } while (row > 1 && renderer.sprite == tiles[column, row - 1].GetComponent<SpriteRenderer>().sprite && renderer.sprite == tiles[column, row - 2].GetComponent<SpriteRenderer>().sprite
                                 || column > 1 && renderer.sprite == tiles[column - 1, row].GetComponent<SpriteRenderer>().sprite && renderer.sprite == tiles[column - 2, row].GetComponent<SpriteRenderer>().sprite
                        );
                        break;
                    case "Medium":
                        //make sure 3 adjacent sprites do not match
                        do
                        {
                            renderer.sprite = availableSprites[Random.Range(0, availableSprites.Count)];
                        } while (row > 1 && renderer.sprite == tiles[column, row - 1].GetComponent<SpriteRenderer>().sprite && renderer.sprite == tiles[column, row - 2].GetComponent<SpriteRenderer>().sprite
                                 || column > 1 && renderer.sprite == tiles[column - 1, row].GetComponent<SpriteRenderer>().sprite && renderer.sprite == tiles[column - 2, row].GetComponent<SpriteRenderer>().sprite
                        );
                        break;
                    case "Hard":
                        //make sure 2 adjacent sprites do not match
                        do
                        {
                            renderer.sprite = availableSprites[Random.Range(0, availableSprites.Count)];
                        } while (row > 0 && renderer.sprite == tiles[column, row - 1].GetComponent<SpriteRenderer>().sprite //sprite matches with Tile above
                                 || column > 0 && renderer.sprite == tiles[column - 1, row].GetComponent<SpriteRenderer>().sprite //sprite matches with Tile to the left
                        );
                        break;
                }

                newTile.AddComponent<BoxCollider2D>().size = Vector2.one;
                Tile tile = newTile.AddComponent<Tile>();
                tile.position = new Vector2Int(column, row);
                
                newTile.transform.parent = transform;
                newTile.transform.position = new Vector3(column * tileWidth, row * tileWidth) + startPos;
                newTile.transform.localScale = Vector3.one * tileWidth * tileScale;

                tiles[column, row] = newTile;
            }
        }
    }
    
    public IEnumerator SwapTiles(Vector2Int tile1Position, Vector2Int tile2Position)
    {
        GameObject tile1 = tiles[tile1Position.x, tile1Position.y];
        SpriteRenderer renderer1 = tile1.GetComponent<SpriteRenderer>();
 
        GameObject tile2 = tiles[tile2Position.x, tile2Position.y];
        SpriteRenderer renderer2 = tile2.GetComponent<SpriteRenderer>();
 
        Sprite temp = renderer1.sprite;
        renderer1.sprite = renderer2.sprite;
        renderer2.sprite = temp;
        
        bool changesOccurs = CheckMatches(false);
        if(!changesOccurs)
        {
            temp = renderer1.sprite;
            renderer1.sprite = renderer2.sprite;
            renderer2.sprite = temp;
            if (FindObjectOfType<AudioManager>() != null && Convert.ToBoolean(PlayerPrefs.GetInt("IsMuted", 0)) == false)
                FindObjectOfType<AudioManager>().Play("Swish");
        }
        else
        {
            if (FindObjectOfType<AudioManager>() != null && Convert.ToBoolean(PlayerPrefs.GetInt("IsMuted", 0)) == false)
                FindObjectOfType<AudioManager>().Play("Pop");
            NumMoves--;
            
            //animate the two tiles
            Vector2 diff = tile1Position - tile2Position;
            if (Math.Abs(diff.x) > Math.Abs(diff.y))
                yield return StartCoroutine(AnimateSwapTilesX(renderer1.transform, renderer2.transform));
            else
                yield return StartCoroutine(AnimateSwapTilesY(renderer1.transform, renderer2.transform));
            
            while (CheckMatches())
            {
                yield return StartCoroutine(FillHoles());
            }
            
            if (NumMoves <= 0) {
                NumMoves = 0;
                GameOver();
            }
            
            if (!AnyMovesLeft()) {
                Toast.Show("You are out of possible moves!", Toast.Position.top, 3f, Toast.Theme.dynamic);
                gameOverReasonText.text = "OUT OF MOVES";
                GameOver();
            }
        }
    }
    
    IEnumerator AnimateSwapTilesX(Transform tile1, Transform tile2, float speed = 2f)
    {
        Vector3 tile1Origin = tile1.position, tile2Origin = tile2.position;
        tile1.position = tile2Origin;    //switch positions
        tile2.position = tile1Origin;    //"""""""
        float delta = tile2.position.x - tile1.position.x;    //distance to traverse
        bool tile1OriginAhead = tile1Origin.x > tile1.position.x;
        
        numAnimatingTiles++;
        do
        {
            tile1.position += Vector3.right * (delta * Time.deltaTime * speed);    //start moving tile1 back to where it came from
            tile2.position -= Vector3.right * (delta * Time.deltaTime * speed);    //start moving tile2 back to where it came from
            yield return new WaitForEndOfFrame();
        } while (tile1OriginAhead? tile1.position.x < tile1Origin.x : tile1.position.x > tile1Origin.x);
        numAnimatingTiles--;
    }
    
    IEnumerator AnimateSwapTilesY(Transform tile1, Transform tile2, float speed = 2f)
    {
        Vector3 tile1Origin = tile1.position, tile2Origin = tile2.position;
        tile1.position = tile2Origin;    //switch positions
        tile2.position = tile1Origin;    //"""""""
        float delta = tile2.position.y - tile1.position.y;    //distance to traverse
        bool tile1OriginAhead = tile1Origin.y > tile1.position.y;
        
        numAnimatingTiles++;
        do
        {
            tile1.position += Vector3.up * (delta * Time.deltaTime * speed);    //start moving tile1 back to where it came from
            tile2.position -= Vector3.up * (delta * Time.deltaTime * speed);    //start moving tile2 back to where it came from
            yield return new WaitForEndOfFrame();
        } while (tile1OriginAhead? tile1.position.y < tile1Origin.y : tile1.position.y > tile1Origin.y);
        numAnimatingTiles--;
    }

    SpriteRenderer GetSpriteRendererAt(int column, int row)
    {
        if (column < 0 || column >= numTiles || row < 0 || row >= numTiles)
            return null;
        GameObject tile = tiles[column, row];
        return tile.GetComponent<SpriteRenderer>();
    }
    
    List<SpriteRenderer> FindColumnMatchForTile(int col, int row, Sprite sprite)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        for (int i = col + 1; i < numTiles; i++)
        {
            SpriteRenderer nextColumn = GetSpriteRendererAt(i, row);
            if (nextColumn.sprite != sprite)
            {
                break;
            }
            result.Add(nextColumn);
        }
        return result;
    }
    
    List<SpriteRenderer> FindRowMatchForTile(int col, int row, Sprite sprite)
    {
        List<SpriteRenderer> result = new List<SpriteRenderer>();
        for (int i = row + 1; i < numTiles; i++)
        {
            SpriteRenderer nextRow = GetSpriteRendererAt(col, i);
            if (nextRow.sprite != sprite)
            {
                break;
            }
            result.Add(nextRow);
        }
        return result;
    }
    
    bool CheckMatches(bool removeMatchedTiles = true)
    {
        HashSet<SpriteRenderer> matchedTiles = new HashSet<SpriteRenderer>();
        for (int row = 0; row < numTiles; row++)
        {
            for (int column = 0; column < numTiles; column++)
            {
                SpriteRenderer current = GetSpriteRendererAt(column, row);
 
                List<SpriteRenderer> horizontalMatches = FindColumnMatchForTile(column, row, current.sprite);
                if (horizontalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(horizontalMatches);
                    matchedTiles.Add(current);
                }
 
                List<SpriteRenderer> verticalMatches = FindRowMatchForTile(column, row, current.sprite);
                if (verticalMatches.Count >= 2)
                {
                    matchedTiles.UnionWith(verticalMatches);
                    matchedTiles.Add(current);
                }
            }
        }

        if (removeMatchedTiles)
        {
            foreach (SpriteRenderer renderer in matchedTiles)
            {
                renderer.sprite = null;
            }
            Score += matchedTiles.Count;
        }
        return matchedTiles.Count > 0;
    }
    
    IEnumerator FillHoles()
    {
        int[] lowestEmptyTiles = {9, 9, 9, 9, 9, 9, 9, 9, 9};
        int[] numEmptyTiles = {0, 0, 0, 0, 0, 0, 0, 0, 0};
        for (int column = 0; column < numTiles; column++)
        {
            for (int row = 0; row < numTiles; row++)
            {
                if (GetSpriteRendererAt(column, row).sprite == null)
                {
                    if (row < lowestEmptyTiles[column])
                        lowestEmptyTiles[column] = row;
                    numEmptyTiles[column]++;
                }
            }
        }
        
        for (int column = 0; column < numTiles; column++)
        {
            for (int row = 0; row < numTiles; row++)
            {
                while (GetSpriteRendererAt(column, row).sprite == null)
                {
                    SpriteRenderer current = GetSpriteRendererAt(column, row);
                    SpriteRenderer next = current;
                    for (int filler = row; filler < numTiles - 1; filler++)
                    {
                        next = GetSpriteRendererAt(column, filler+1);
                        current.sprite = next.sprite;
                        current = next;
                    }
                    next.sprite = availableSprites[Random.Range(0, availableSprites.Count)];
                }
            }
        }
        
        //drop tiles
        int waitCol = -1, waitRow = -1;
        for (int column = 0; column < numTiles; column++)
        {
            if (numEmptyTiles[column] > 0)
            {
                for (int row = lowestEmptyTiles[column]; row < numTiles; row++)
                {
                    if (column == Array.IndexOf(numEmptyTiles, numEmptyTiles.Max()) && row == lowestEmptyTiles[column])
                    {
                        waitCol = column;
                        waitRow = row;
                    }
                    else
                    {
                        StartCoroutine(DropTile(tiles[column, row].transform, numEmptyTiles[column]));
                    }
                }
            }
        }
        
        if(waitCol != -1 && waitRow != -1)
            yield return StartCoroutine(DropTile(tiles[waitCol, waitRow].transform, numEmptyTiles[waitCol]));
        else 
            yield break;
    }

    IEnumerator DropTile(Transform tile, int numRows, float speed = 2f)
    {
        float finalTilePos = tile.position.y;
        tile.position += Vector3.up * tileWidth * numRows;
        float orgTilePos = tile.position.y;
        float delta = Math.Abs(finalTilePos - orgTilePos);
        
        numAnimatingTiles++;
        while (tile.position.y > finalTilePos)
        {
            tile.position += Vector3.down * (delta * Time.deltaTime * speed/numRows);
            yield return new WaitForEndOfFrame();
        }

        numAnimatingTiles--;
    }

    bool AnyMovesLeft()
    {
        int possibleMoves = 0;
        Sprite[][] sprites = new Sprite[numTiles][];
        for (int column = 0; column < numTiles; column++)
        {
            sprites[column] = new Sprite[numTiles];
            for (int row = 0; row < numTiles; row++)
            {
                sprites[column][row] = tiles[column, row].GetComponent<SpriteRenderer>().sprite;
            }
        }

        for (int column=0; column<numTiles; column++)
        {
            for (int row=0; row<numTiles; row++)
            {
                //first combination (3,2)
                if (column <= numTiles - 3
                    && row <= numTiles - 2
                    && (sprites[column][row] == sprites[column+1][row] && sprites[column+1][row] == sprites[column+2][row+1]
                        || sprites[column][row+1] == sprites[column+1][row] && sprites[column+1][row] == sprites[column+2][row]
                        || sprites[column][row+1] == sprites[column+1][row+1] && sprites[column+1][row+1] == sprites[column+2][row]
                        || sprites[column][row] == sprites[column+1][row+1] && sprites[column+1][row+1] == sprites[column+2][row+1]
                        || sprites[column][row+1] == sprites[column+1][row] && sprites[column+1][row] == sprites[column+2][row+1]
                        || sprites[column][row] == sprites[column+1][row+1] && sprites[column+1][row+1] == sprites[column+2][row]))
                    possibleMoves++;
                
                //second combination (2,3)
                if (column <= numTiles - 2
                    && row <= numTiles - 3
                    && (sprites[column][row] == sprites[column][row+1] && sprites[column][row+1] == sprites[column+1][row+2]
                        || sprites[column+1][row] == sprites[column][row+1] && sprites[column][row+1] == sprites[column][row+2]
                        || sprites[column+1][row] == sprites[column+1][row+1] && sprites[column+1][row+1] == sprites[column][row+2]
                        || sprites[column][row] == sprites[column+1][row+1] && sprites[column+1][row+1] == sprites[column+1][row+2]
                        || sprites[column][row] == sprites[column+1][row+1] && sprites[column+1][row+1] == sprites[column][row+2]
                        || sprites[column+1][row] == sprites[column][row+1] && sprites[column][row+1] == sprites[column+1][row+2]))
                    possibleMoves++;
                
                //third combination (4,1)
                if (row <= numTiles - 4
                    && (sprites[column][row] == sprites[column][row+1] && sprites[column][row+1] == sprites[column][row+3]
                        || sprites[column][row] == sprites[column][row+2] && sprites[column][row+2] == sprites[column][row+3]))
                    possibleMoves++;
                
                //fourth combination (1,4)
                if (column <= numTiles - 4
                    && (sprites[column][row] == sprites[column+1][row] && sprites[column+1][row] == sprites[column+3][row]
                        || sprites[column][row] == sprites[column+2][row] && sprites[column+2][row] == sprites[column+3][row]))
                    possibleMoves++;
            }
        }
        return possibleMoves > 0;
    }
    
    void GameOver()
    {
        numAnimatingTiles++;
        int highScore = PlayerPrefs.GetInt ($"{SceneManager.GetActiveScene().name} score", 0);
        if (Score > highScore)
        {
            PlayerPrefs.SetInt($"{SceneManager.GetActiveScene().name} score", Score);
            Toast.Show("New High Score!!", Toast.Position.bottom, 3f, Toast.Theme.dynamic);
        }
        highScore = PlayerPrefs.GetInt ($"{SceneManager.GetActiveScene().name} score", 0);
        highScoreText.text = highScore.ToString();
        if (FindObjectOfType<AudioManager>() != null && Convert.ToBoolean(PlayerPrefs.GetInt("IsMuted", 0)) == false)
            FindObjectOfType<AudioManager>().Play("Jingle");
        gameOverMenu.SetActive (true);
    }
}
