﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;


public class Main : MonoBehaviour {

    private int noiseCount;
    private Color[][] images;
    GeneticAlgo geneticAlgo;
    Text textGeneration, textScore0, textEvolve, textReset;
    InputField iField;
    public string inputSelection;
    public int width;
    public int height;
    public int rawCount = 20;

    public bool enable, validChoice;

    // Use this for initialization
    void Awake () {
        Run();
      }

    public void Run()
    {
        enable = false;
        width = 8;
        height = 8;

        RawImage[] rawImages = GetComponentsInChildren<RawImage>();
        textGeneration = GameObject.Find("TextGeneration").GetComponent<Text>();
        //textScore0 = GameObject.Find("TextScore0").GetComponent<Text>();
        textEvolve = GameObject.Find("TextEvolve").GetComponent<Text>();
        textReset = GameObject.Find("TextReset").GetComponent<Text>();
        iField = GameObject.Find("InputSelection").GetComponent<InputField>();

        textReset.text = "Reset";


        //currently my images take  values 0.0, 0.1, ...... 0.9
        //i should remap these numbers to a letters from a to...

        //sets the last random image as the target image for now.....

        System.Random r = new System.Random(Time.frameCount);
        string target = colorsToString(TextureNoise.CreateNoise(width, height, r));

        GetInput();
        startEvo(target);

        
        //holds the candidate images
        noiseCount = 20;
        images = new Color[noiseCount][];

        //for (int i = 0; i < noiseCount; i++)
        //{
        //    System.Random r = new System.Random(i + Time.frameCount);
        //    images[i] = TextureNoise.CreateNoise(width, height, r);
        //}
        //applies the images to the rawImages in GUI and add click handler to raw images
        for (int i = 0; i < rawCount; i++)
        {
            Color[] test = stringToColors(geneticAlgo.Population._genomes[i].genome);
            TextureDisplay.applyTexture(test, rawImages[i], width, height);
            rawImages[i].gameObject.AddComponent<Button>();
            Button btn = rawImages[i].gameObject.GetComponent<Button>();
            string txt = rawImages[i].gameObject.name;

            btn.onClick.AddListener(() => OnClickButton(txt));
            btn.transition = Selectable.Transition.ColorTint;

            ColorBlock cb = rawImages[i].gameObject.GetComponent<Button>().colors;
            cb.pressedColor = new Color(0.2f, 0.2f, 0.2f);
            btn.colors = cb;
        }

        //RawImage rawTarget = GameObject.Find("RawTarget").GetComponent<RawImage>();
        //TextureDisplay.applyTexture(images[noiseCount - 1], rawTarget, width, height);



        //need to draw the images after the initial population has been generated

        //how to improve evo, the random in new genomes is not changing, the equals thing where the same, parse the inputs!!!!!!
    }

    public void OnClickButton(string rawSelectionNum)
    {
        iField.text = rawSelectionNum.Substring(3).ToString() + " " + iField.text;
        CheckInputCount();
    }

    //ensures that the number of entries in the input field is 5
    public void CheckInputCount()
    {
        string[] inputs = iField.text.Split(' ');
        string[] checkedInputs = inputs.Length > 5 ? inputs.Take(5).ToArray() : inputs;
        iField.text = string.Join(" ", checkedInputs);
    }

    public string colorsToString(Color[] pixels)
    {
        char[] chars = new char[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            chars[i] = (char)(pixels[i].r * 10 + 97);
        }

        string output = new string(chars);

        Debug.Log("target string is " + output);

        return output;
    }

    public Color[] stringToColors(string guess)
    {
        Color[] colors = new Color[guess.Length];
        char[] chars = guess.ToCharArray();

        for (int i = 0; i < guess.Length; i++)
        {
            float colorW = (chars[i] - 97) / 10f;
            colors[i] = new Color(colorW, colorW, colorW);
        }

        return colors;
    }

    public void startEvo(string target)
    {
        string selectType = "god mode";
        string mutateType = "randomChoice";
        string crossType = "OnePt";

        Fitness fitness = new Fitness(target);
        Population population = new Population(20, fitness._targetString) { _name = "images" };
        Selection selection = new Selection(selectType);
        CrossOver crossover = new CrossOver(crossType);
        Mutation mutation = new Mutation(mutateType);

        geneticAlgo = new GeneticAlgo(fitness, population, selection, crossover, mutation) ;
    }

    public void OnClickEvolve()
    {
        if(geneticAlgo == null) { Run(); }

        enable = !enable;

        if (enable == true)
        {
            textEvolve.text = "<Color=#000000>Pause</color>";
        }
        else
        {
            textEvolve.text = "Evolve";
        }
    }

    //button to reset the algo
    public void OnClickReset()
    {
        enable = false;
        geneticAlgo = null;
        textReset.text = " <---- Press";
        Run();
        textGeneration.text = "Generation : " + geneticAlgo.Population._generation.ToString();
    }

    //button to quit app
    public void OnClickExit(){
        Application.Quit();
    }

    public void GetInput(){
        inputSelection = iField.text;
        validChoice = true;

        if (iField.text.Split(' ').Length < 5)
        {
            validChoice = false;
            enable = false;
            Debug.Log("Must select 5 candidates");
            textEvolve.text = "Evolve";
        }
    }

    // Update is called once per frame
    void Update () {
        if(enable){
        if(geneticAlgo.Selection._selectionType == "god mode"){
            UpdateUser();
        }
        else{
            UpdateFitness();
        }
        }

    }

    //update function for user selection based algorithm
    public void UpdateUser()
    {
        GetInput();
        if (enable && validChoice)
        {

            string finalOutput = "";
            finalOutput = geneticAlgo.ToString();
            geneticAlgo.NextGeneration(inputSelection);
            enable = false; //pausing the simulation to get new data
            textEvolve.text = "Evolve";
            iField.text = "";
            Debug.Log(finalOutput);

            //get the first 5 candidates from this generations genomes
            for (int i = 0; i < rawCount; i++)
            {
                string scoreNum = "TextScore" + i.ToString();
                string rawNum = "Raw" + i.ToString();
                Color[] test = stringToColors(geneticAlgo.Population._genomes[i].genome);
                RawImage raw = GameObject.Find(rawNum).GetComponent<RawImage>();
                //Text score = GameObject.Find(scoreNum).GetComponent<Text>();
                TextureDisplay.applyTexture(test, raw, width, height);
            }
            textGeneration.text = "Generation : " + geneticAlgo.Population._generation.ToString();
        }

    }


    //update function for fitness based algorithm
    public void UpdateFitness(){
    
        if (enable && geneticAlgo.Population._bestFitness < 64 && Time.frameCount < 10000)
        {
            string finalOutput = "";
            finalOutput = geneticAlgo.ToString();
            geneticAlgo.NextGeneration(inputSelection);
            textEvolve.text = "Evolve";
            Debug.Log(finalOutput);

            //get the top 5 candidates from this generations genomes
            for (int i = 0; i < rawCount; i++)
            {
                string scoreNum = "TextScore" + i.ToString();
                string rawNum = "Raw" + i.ToString();
                Color[] test = stringToColors(geneticAlgo.Population._genomes[i].genome);
                RawImage raw = GameObject.Find(rawNum).GetComponent<RawImage>();
                //Text score = GameObject.Find(scoreNum).GetComponent<Text>();
                TextureDisplay.applyTexture(test, raw, width, height);
                //score.text = "Fitness : " + geneticAlgo.Population._fitnesses[i].ToString() + "/64";
            }

            textGeneration.text = "Generation : " + geneticAlgo.Population._generation.ToString();

        } 
        else if (enable && geneticAlgo.Population._bestFitness == 64 && Time.frameCount < 10000)

        {
            for (int i = 0; i < 5; i++)
            {
                if (geneticAlgo.Population._fitnesses[i] == 64) {

                string scoreNum = "TextScore" + i.ToString();
                Text score = GameObject.Find(scoreNum).GetComponent<Text>();
                score.text = "Evolved!";
            }
            }
        }
    }






}


