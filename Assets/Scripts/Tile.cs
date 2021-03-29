using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private GridManager gridMgr;
    private static Tile selected;
    private float delta, thresh, animSpeed;
    public Vector2Int position;
    private SpriteRenderer sprite;
    
    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        gridMgr = transform.root.GetComponent<GridManager>();
        thresh = gridMgr.tileWidth * 0.12f;
    }
    
    private void Update()
    {
        transform.position += Vector3.up * (animSpeed * thresh * Time.deltaTime);
        delta += animSpeed * thresh * Time.deltaTime;
        if (delta > thresh) //if we've moved beyond the threshold
        {
            animSpeed = -3f;    //start moving the opposite direction
        }

        if (delta < -thresh)
        {
            animSpeed = 3f;
        }
        
        //don't render tile if it is outside the grid
        if(transform.position.y > gridMgr.tileWidth * gridMgr.numTiles / 2)
            sprite.color = new Color(1,1,1,0);
        else
            sprite.color = new Color(1,1,1,1);
    }
 
    public void Select()
    {
        animSpeed = 3f;    //animate
    }
 
    public void Unselect()
    {
        animSpeed = 0f;    //stop animating
        transform.position -= Vector3.up * delta;    //reset position
        delta = 0f;
    }
 
    private void OnMouseDown()
    {
        if (gridMgr.numAnimatingTiles == 0)
        {
            if (selected != null) //if there's a selected tile
            {
                selected.Unselect(); //stop animating the selected tile
            }

            if (selected == this) //if this tile is the selected tile
            {
                selected = null; //let go of this tile
                if (FindObjectOfType<AudioManager>() != null &&
                    Convert.ToBoolean(PlayerPrefs.GetInt("IsMuted", 0)) == false)
                    FindObjectOfType<AudioManager>().Play("Select");
            }
            else
            {
                if (selected != null && Vector2Int.Distance(selected.position, position) == 1) //if adjacent
                {
                    StartCoroutine(gridMgr.SwapTiles(position, selected.position)); //swap tiles
                    selected = null; //let go of this tile
                }
                else
                {
                    selected = this; //select this tile
                    Select(); //start animating this tile
                    if (FindObjectOfType<AudioManager>() != null &&
                        Convert.ToBoolean(PlayerPrefs.GetInt("IsMuted", 0)) == false)
                        FindObjectOfType<AudioManager>().Play("Select");
                }
            }
        }
    }
}