using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    private int noiseCount;
    private Color[][] images;
    GeneticAlgo geneticAlgo;
    Text textGeneration, textScore0, textEvolve, textReset;
    InputField iField;
    public string inputSelection;
    public int width;
    public int height;

    public bool enable;

    // Use this for initialization
    void Awake () {
        Run();
      }

    public void Run()
    {
        width = 8;
        height = 8;
        enable = false;

        RawImage[] rawImages = GetComponentsInChildren<RawImage>();
        textGeneration = GameObject.Find("TextGeneration").GetComponent<Text>();
        textScore0 = GameObject.Find("TextScore0").GetComponent<Text>();
        textEvolve = GameObject.Find("TextEvolve").GetComponent<Text>();
        textReset = GameObject.Find("TextReset").GetComponent<Text>();
        iField = GameObject.Find("InputSelection").GetComponent<InputField>();

        textReset.text = "Reset";

        //holds the candidate images
        noiseCount = 20;
        images = new Color[noiseCount][];

        for (int i = 0; i < noiseCount; i++)
        {
            System.Random r = new System.Random(i + Time.frameCount);
            images[i] = TextureNoise.CreateNoise(width, height, r);
        }
        //applies the images to the rawImages in GUI
        for (int i = 0; i < 5; i++)
        {
            TextureDisplay.applyTexture(images[i], rawImages[i], width, height);
        }

        RawImage rawTarget = GameObject.Find("RawTarget").GetComponent<RawImage>();
        TextureDisplay.applyTexture(images[noiseCount - 1], rawTarget, width, height);

        //currently my images take  values 0.0, 0.1, ...... 0.9
        //i should remap these numbers to a letters from a to...

        //sets the last random image as the target image for now.....
        string target = colorsToString(images[noiseCount - 1]);
        GetInput();
        startEvo(target);

        //how to improve evo, the random in new genomes is not changing, the equals thing where the same, parse the inputs!!!!!!
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
        Population population = new Population(5000, fitness._targetString) { _name = "images" };
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
    }

    //button to quit app
    public void OnClickExit()
    {
        Application.Quit();
    }

    public void GetInput(){
        inputSelection = iField.text;
    }

    // Update is called once per frame
    void Update () {

        if (enable && geneticAlgo.Population._bestFitness < 64 && Time.frameCount < 10000)
        {
            string finalOutput = "";
            finalOutput = geneticAlgo.ToString();
            geneticAlgo.NextGeneration(inputSelection);
            enable = false;
            GetInput();
            textEvolve.text = "Evolve";
            Debug.Log(finalOutput);

            //get the top 5 candidates from this generations genomes
            for (int i = 0; i < 5; i++)
            {
                string scoreNum = "TextScore" + i.ToString();
                string rawNum = "Raw" + i.ToString();
                Color[] test = stringToColors(geneticAlgo.Population._genomes[i].genome);
                RawImage raw = GameObject.Find(rawNum).GetComponent<RawImage>();
                Text score = GameObject.Find(scoreNum).GetComponent<Text>();
                TextureDisplay.applyTexture(test, raw, width, height);
                //score.text = "Fitness : " + geneticAlgo.Population._fitnesses[i].ToString() + "/64";

            }

            textGeneration.text = "Generation : " + geneticAlgo.Population._generation.ToString();

        } 
        //else if (enable && geneticAlgo.Population._bestFitness == 64 && Time.frameCount < 10000)

        //{
        //    for (int i = 0; i < 5; i++)
        //    {
        //        if (geneticAlgo.Population._fitnesses[i] == 64) {

        //        string scoreNum = "TextScore" + i.ToString();
        //        Text score = GameObject.Find(scoreNum).GetComponent<Text>();
        //        score.text = "Evolved!";
        //    }
        //    }
        //}
    }
}


