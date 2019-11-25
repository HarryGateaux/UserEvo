using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Main : MonoBehaviour {

    RawImage[] rawImages;
    GeneticAlgo geneticAlgo;
    Text textGeneration, textScore0, textEvolve, textReset;
    InputField inputSelection;
    public int width;
    public int height;
    public int rawCount = 20;

    public bool enable, validChoice;

    // Use this for initialization
    void Awake () {

        rawImages = GetComponentsInChildren<RawImage>();

        textGeneration = GameObject.Find("TextGeneration").GetComponent<Text>();
        //textScore0 = GameObject.Find("TextScore0").GetComponent<Text>();
        textEvolve = GameObject.Find("TextEvolve").GetComponent<Text>();
        textReset = GameObject.Find("TextReset").GetComponent<Text>();

        inputSelection = GameObject.Find("InputSelection").GetComponent<InputField>();

        for (int i = 0; i < rawCount; i++)
        {
            //add a button to each image for selection
            rawImages[i].gameObject.AddComponent<Button>();
            Button btn = rawImages[i].gameObject.GetComponent<Button>();
            string txt = rawImages[i].gameObject.name;
            btn.onClick.AddListener(() => OnClickImage(txt));
            //formatting the button on click
            ColorBlock cb = rawImages[i].gameObject.GetComponent<Button>().colors;
            cb.pressedColor = new Color(0.2f, 0.2f, 0.2f);
            btn.colors = cb;
        }

        Setup();
    
    }
    
    public void Setup()
    {
        enable = false;
        width = 8;
        height = 8;

        System.Random r = new System.Random(Time.frameCount);
        string target = colorsToString(TextureNoise.CreateNoise(width, height, r));

        GetUserSelection();
        CreateGA(target);
        DisplayPhenotypes(rawCount);

        textEvolve.text = "Evolve";
        textReset.text = "Reset";
        textGeneration.text = "Generation : " + geneticAlgo.Population._generation.ToString();

    }

    public void DisplayPhenotypes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            //convert initial genomes to images
            Color[] specimen = stringToColors(geneticAlgo.Population._genomes[i].genome);
            TextureDisplay.applyTexture(specimen, rawImages[i], width, height);
        }
    }


    public void CreateGA(string target)
    {
        string selectType = "god mode";
        string mutateType = "randomChoice";
        string crossType = "OnePt";

        Fitness fitness = new Fitness(target);
        Population population = new Population(20, fitness._targetString) { _name = "images" };
        Selection selection = new Selection(selectType);
        CrossOver crossover = new CrossOver(crossType);
        Mutation mutation = new Mutation(mutateType);

        geneticAlgo = new GeneticAlgo(fitness, population, selection, crossover, mutation);
    }

    //encodes the color to a string genome
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

    //decodes of a string genome to a color
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

    public void OnClickEvolve()
    {

        if(geneticAlgo.Stopped == true) { Setup();}

        enable = !enable;

        if (enable == true)
            textEvolve.text = "<Color=#000000>Pause</color>";
        else
            textEvolve.text = "Evolve";
    }

    //button to reset the algo
    public void OnClickReset()
    {
        enable = false;
        geneticAlgo.Stopped = true;

        Setup();
    }

    public void OnClickExit(){
        Application.Quit();
    }

    public void OnClickImage(string rawSelectionNum)
    {
        inputSelection.text = rawSelectionNum.Substring(3).ToString() + " " + inputSelection.text;
        CheckInputCount();
    }

    //ensures that the number of entries in the input field is 5 before allowing next generation
    public void CheckInputCount()
    {
        string[] inputs = inputSelection.text.Split(' ');
        string[] checkedInputs = inputs.Length > 5 ? inputs.Take(5).ToArray() : inputs;
        inputSelection.text = string.Join(" ", checkedInputs);
    }

    public void GetUserSelection(){
        validChoice = true;

        if (inputSelection.text.Split(' ').Length < 5)
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
        GetUserSelection();
        
        if (enable && validChoice)
        {
            string finalOutput = "";
            finalOutput = geneticAlgo.ToString();
            geneticAlgo.NextGeneration(inputSelection.text);
            enable = false; //pausing the simulation to get new data
            textEvolve.text = "Evolve";
            inputSelection.text = "";
            Debug.Log(finalOutput);

            //get the first 5 candidates from this generations genomes
            //for (int i = 0; i < rawCount; i++)
            //{
            //    string scoreNum = "TextScore" + i.ToString();
            //    string rawNum = "Raw" + i.ToString();
            //    Color[] test = stringToColors(geneticAlgo.Population._genomes[i].genome);
            //    RawImage raw = GameObject.Find(rawNum).GetComponent<RawImage>();
            //    //Text score = GameObject.Find(scoreNum).GetComponent<Text>();
            //    TextureDisplay.applyTexture(test, raw, width, height);
            //}

            DisplayPhenotypes(rawCount);
                                          
            textGeneration.text = "Generation : " + geneticAlgo.Population._generation.ToString();
        }

    }

    //update function for fitness based algorithm
    public void UpdateFitness(){
    
        if (enable && geneticAlgo.Population._bestFitness < 64 && Time.frameCount < 10000)
        {
            string finalOutput = "";
            finalOutput = geneticAlgo.ToString();
            geneticAlgo.NextGeneration(inputSelection.text);
            textEvolve.text = "Evolve";
            Debug.Log(finalOutput);

            //get the top 5 candidates from this generations genomes
            //for (int i = 0; i < rawCount; i++)
            //{
            //    string scoreNum = "TextScore" + i.ToString();
            //    string rawNum = "Raw" + i.ToString();
            //    Color[] test = stringToColors(geneticAlgo.Population._genomes[i].genome);
            //    RawImage raw = GameObject.Find(rawNum).GetComponent<RawImage>();
            //    //Text score = GameObject.Find(scoreNum).GetComponent<Text>();
            //    TextureDisplay.applyTexture(test, raw, width, height);
            //    //score.text = "Fitness : " + geneticAlgo.Population._fitnesses[i].ToString() + "/64";
            //}

            DisplayPhenotypes(rawCount);
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


