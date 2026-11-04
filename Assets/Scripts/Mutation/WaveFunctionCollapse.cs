using NUnit.Framework;
using System.Collections.Generic;
//using System.Drawing;
using UnityEngine;
using UnityEngine.Rendering;

public enum WFCDomenant 
{ 
    fatherDominent,
    motherDominent, 
    perfectMix
}

public class WFCcell 
{
    private bool isCollapsed;
    private int entropy = 0;
    private Color cellColor = Color.red;
    private WFCDomenant cellsDominentParent = WFCDomenant.perfectMix;
    private List<WFCDomenant> activeChoices = new List<WFCDomenant>();
    private int gridPosX;
    private int gridPosY;

    public bool IsCollapsed 
    { 
        get => isCollapsed;
        set => isCollapsed = value;
    }
    public int Entropy 
    { 
        get => entropy;
        set => entropy = value;
    }
    public Color CellColor
    {
        get => cellColor;
        set => cellColor = value;
    }

    public List<WFCDomenant> ActiveChoices 
    { 
        get => activeChoices;
        set => activeChoices = value;
    }

    public WFCDomenant CellsDominentParent 
    {
        get => cellsDominentParent;
        set => cellsDominentParent = value;
    }

    public int GridPosX 
    { 
        get => gridPosX;
        set => gridPosX = value;
    }
    public int GridPosY 
    {
        get => gridPosY;
        set => gridPosY = value;
    }
}

public class WaveFunctionCollapse : MonoBehaviour
{

    Texture2D fatherTexture;
    Texture2D motherTexture;
    Texture2D newTexture;
    List<WFCcell> isnetCollapsedCells = new List<WFCcell>();

    WFCcell[,] cells;

    List<Color> possibleColorsFather;
    List<Color> possibleColorsMother;
    List <Color> possibleColorsPerfectMix; // Will act as a buffert between the dominent forces

    //DebugValues
    [SerializeField] private GameObject debugObj;
    [SerializeField] private bool activateDebug;

    void GetPossibleColors(Texture2D fatherTex, Texture2D motherTex, WFCcell[,] cells, List<WFCcell> isnetCollapsed) 
    {

        possibleColorsFather = new List<Color>();
        possibleColorsMother = new List<Color>();
        possibleColorsPerfectMix = new List<Color>(); // Will act as a buffert between the dominent forces

        Color[] fatherPixels = fatherTex.GetPixels();
        Color[] motherPixels = motherTex.GetPixels();

        cells = new WFCcell[fatherTex.width, fatherTex.height];

        PopulateGridWithCells(cells, fatherTex.width, fatherTex.height, isnetCollapsed);

        for(int i = 0; i < fatherPixels.Length; i++) 
        {
            if (!possibleColorsFather.Contains(fatherPixels[i]))
            {
                possibleColorsFather.Add(fatherPixels[i]);
            }

        }

        for (int i = 0; i < motherPixels.Length; i++)
        {
            if (!possibleColorsMother.Contains(motherPixels[i]))
            {
                possibleColorsMother.Add(motherPixels[i]);
            }
        }

        //This will add a third list that acts as a buffer between the two different colors
        for (int i = 0; i < possibleColorsMother.Count; i++) 
        {
            if (possibleColorsFather[i] == null) return;
            possibleColorsPerfectMix.Add(Color.Lerp(possibleColorsMother[i], possibleColorsFather[i], Random.Range(0.3f, 0.6f))); //Adds some variaty to the streangth
        }
    }

    void PopulateGridWithCells(WFCcell[,] array, int width, int height, List<WFCcell> isnetCollapsedCells) 
    {
        WFCDomenant father = WFCDomenant.fatherDominent;
        WFCDomenant mother = WFCDomenant.fatherDominent;
        WFCDomenant mix = WFCDomenant.fatherDominent;
        List<WFCDomenant> list = new List<WFCDomenant> { father, mother, mix };

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                WFCcell cell = new WFCcell();
                cell.GridPosX = i;
                cell.GridPosY = j;
                cell.ActiveChoices = list;
                cell.Entropy = list.Count;
                isnetCollapsedCells.Add(cell);

                array[i, j] = cell;
            }
        }
    }

    //void PopulateCellChoices(WFCcell[,] cells, List<WFCcell> isnetCollapsedCells) 
    //{
    //    //List<WFCDomenant> list = new List<WFCDomenant> {WFCDomenant.fatherDominent, WFCDomenant.motherDominent, WFCDomenant.perfectMix };


    //    foreach (WFCcell cell in cells) 
    //    {
    //        cell.ActiveChoices = list;
    //        cell.Entropy = list.Count;
    //        isnetCollapsedCells.Add(cell); //Needed to check that all cells are collapsed

    //    }
    //}

    //Will be used to initionaly populate the textures
    Color GetRandomColorFromList(List<Color> list) 
    {
        Color color = new Color();
        color = list[Random.Range(0, list.Count)];
        return color;
    }

    WFCcell GetLowestEntropy() 
    {
        if (isnetCollapsedCells.Count == 0) return null;

        WFCcell tempCell = cells[0,0];
        foreach(WFCcell cell in cells) 
        {
            if (tempCell == cell || cell.IsCollapsed) continue;
            if(cell.Entropy < tempCell.Entropy) 
            {
                tempCell = cell;
            }
        }
        return tempCell;
    }

    void CollapseCell(WFCcell cell, List<WFCcell> insetCollapsedCells) 
    {
        cell.CellsDominentParent = cell.ActiveChoices[Random.Range(0, cell.ActiveChoices.Count)];
        cell.IsCollapsed = true;
        insetCollapsedCells.Remove(cell);

    }

    void RulesOfNeighbor(WFCcell cell, int width, int height) //Propegering?
    {
        if (cell.IsCollapsed) return;

        WFCDomenant selectedDomenent = cell.CellsDominentParent;
        List<WFCcell> neighbor = new List<WFCcell>();
        if (cell.GridPosX < width -1 && cell.GridPosY < height - 1 && cell.GridPosX > 1 && cell.GridPosY > 1) 
        {
            for (int i = -1; i < 1; i++)
            {
                for (int j = -1; j < 1; j++)
                {
                    if (i == 0 && j == 0) return;

                    WFCcell currentCell = cells[cell.GridPosX + i, cell.GridPosY + j];
                    if (!currentCell.IsCollapsed)
                    {
                        neighbor.Add(cells[cell.GridPosX + i, cell.GridPosY + j]);
                    }
                }
            }

            if (cell.CellsDominentParent == WFCDomenant.motherDominent)
            {
                foreach (WFCcell wFCcell in neighbor)
                {
                    wFCcell.ActiveChoices.Remove(WFCDomenant.fatherDominent);
                    wFCcell.Entropy = wFCcell.ActiveChoices.Count;
                }
            }
            else if (cell.CellsDominentParent == WFCDomenant.fatherDominent)
            {
                foreach (WFCcell wFCcell in neighbor)
                {
                    wFCcell.ActiveChoices.Remove(WFCDomenant.motherDominent);
                    wFCcell.Entropy = wFCcell.ActiveChoices.Count;
                }
            } 
        }
        else 
        { 
            return;
        }
    }

    public void WFC(Texture2D fatherTexture, Texture2D motherTexture, WFCcell[,] cells)
    {
        GetPossibleColors(fatherTexture, motherTexture, cells, isnetCollapsedCells);
        //PopulateCellChoices(cells);

        while(isnetCollapsedCells.Count != 0) 
        {
            WFCcell cell = GetLowestEntropy();
            if (cell != null) 
            {
                CollapseCell(cell, isnetCollapsedCells);
                RulesOfNeighbor(cell, fatherTexture.width, fatherTexture.height);
            }
            else 
            {
                isnetCollapsedCells.Clear(); //Tried to Failsafe
            }
        }

        PaintTheFinalResult(cells, fatherTexture.width, fatherTexture.height);
        CreateNewTexture(newTexture, cells);

    }


    void PaintTheFinalResult(WFCcell[,] cells, int width, int height) 
    {

        for (int i = 0; i < width - 1; i++)
        {
            for (int j = 0; j < height - 1; j++)
            {

                //cells[i, j].CellColor = GetRandomColorFromList(possibleColorsMother);
                if (cells[i, j].CellsDominentParent == WFCDomenant.motherDominent)
                {
                    cells[i, j].CellColor = GetRandomColorFromList(possibleColorsMother);
                }
                else if (cells[i, j].CellsDominentParent == WFCDomenant.fatherDominent)
                {
                    cells[i, j].CellColor = GetRandomColorFromList(possibleColorsFather);
                }
                else
                {
                    cells[i, j].CellColor = GetRandomColorFromList(possibleColorsPerfectMix);
                }

            }
        }
    }

    void CreateNewTexture(Texture2D size, WFCcell[,] cells) 
    {
        newTexture = new Texture2D(256, 256);
        for (int i = 0; i < size.width; i ++) 
        { 
            for (int j = 0; j < size.height; j++) 
            {
                Color setColor = cells[i, j].CellColor;
                newTexture.SetPixel(i, j, setColor);
            }
        }
        newTexture.Apply();
    }

    /// <summary>
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// <param name="fatherTex"></param>
    /// <param name="motherTex"></param>
    /// 

    //Debug feature, making textures inside this scripts to see if it works
    void DebugCreateTextures(Texture2D fatherTex, Texture2D motherTex) 
    {
        Texture2D texture = new Texture2D(256,256);

        for(int i = 0; i < texture.width; i++) 
        { 
            for (int j = 0; j < texture.height; j++) 
            {
                fatherTex.SetPixel(i, j, Color.blue);
                motherTex.SetPixel(i, j, Color.red);
            }
        }
        fatherTex.Apply();
        motherTex.Apply();
    }

    Texture2D DebugCreateFather() 
    { 
        Texture2D fatherTex = new Texture2D(256,256);
        return fatherTex;
    }
    Texture2D DebugCreateMother()
    {
        Texture2D motherTex = new Texture2D(256, 256);
        return motherTex;
    }

    void DebugSetobjColor(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        renderer.material.mainTexture = newTexture;
    }

    public void DebugWFC()
    {
        List<WFCcell> isnetCollapsedCells = new List<WFCcell>();
        Texture2D fatherTex = DebugCreateFather();
        Texture2D motherTex = DebugCreateMother();
        DebugCreateTextures(fatherTex, motherTex);

        WFCcell[,] cells = new WFCcell[256, 256];
        GetPossibleColors(fatherTex, motherTex, cells, isnetCollapsedCells);


        while (isnetCollapsedCells.Count != 0)
        {
            WFCcell cell = GetLowestEntropy();
            if (cell != null)
            {
                CollapseCell(cell, isnetCollapsedCells);
                RulesOfNeighbor(cell, fatherTexture.width, fatherTexture.height);
            }
            else
            {
                isnetCollapsedCells.Clear(); //Tried to Failsafe
            }
        }
        PaintTheFinalResult(cells, fatherTex.width, fatherTex.height);
        CreateNewTexture(newTexture, cells);
        DebugSetobjColor(debugObj);

    }

    private void Update()
    {
        if(activateDebug == true) 
        {
            activateDebug = false;
            DebugWFC();
        }
    }
}
