using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Valkyrie.EIR.Haptics;
using Valkyrie.EIR;
using Valkyrie.EIR.Interaction;
using TMPro;

public class DowsingGame : MonoBehaviour {
    /*
    #region EMS related variables

    HapticPreset[] buriedObjectProperties;

    (Vector3, HapticPresetRunner, HapticPresetRunner)[] positionAndRunners = new (Vector3, HapticPresetRunner, HapticPresetRunner)[0];

    GameObject lHand, rHand;

    #endregion

    #region Game related variables

    Vector3[] objectPositions = new Vector3[]
    {
        new Vector3(0,4.3f,2.5f),
        new Vector3(2,4.2f,1.5f),
        new Vector3(-2,4.2f,1.5f),
        new Vector3(-3,4.5f,0.5f),
        new Vector3(3,4f,0.5f)
    };

    [SerializeField]
    GameObject signPrefab, buttonPrefab, nextStageButton, canvas;

    [SerializeField]
    TextMeshProUGUI text, scoreText;

    [SerializeField]
    Sprite[] signSprites;

    int score = 0;

    enum BuriedObjects {
        nothing,
        water,
        snake,
        waterSnake
    }

    int roundCount = 0;

    List<GameObject> activeSigns = new List<GameObject>();
    List<GameObject> activeButtons = new List<GameObject>();
    List<BuriedObjects> activeBuriedObjects = new List<BuriedObjects>();

    #endregion


    // Setup presets and find hands before beginning the game
    void Start() {
        SetupPresetProperties();
#if EIR_INTERACTION
        lHand = EIRManager.Instance.Interaction.InteractingBodyParts[(int)BodyPart.leftHand].gameObject;
        rHand = EIRManager.Instance.Interaction.InteractingBodyParts[(int)BodyPart.rightHand].gameObject;
#endif
        Invoke("SelectRound", 0.1f);
    }

    // Modify the intensity of all the different active presets
    void Update() {
        HandleIntensities();
    }


    #region EMS

    //Create presets for the objects we will be finding
    void SetupPresetProperties() {
        //Create preset properties for all of the buried objects
        HapticPreset nothing = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.minimum, 1, true);
        HapticPreset water = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine, 1, true);

        //Snake preset is custom! Designed to feel spiky and hostile
        HapticPreset snake = new HapticPreset();
        snake.m_segments = new HapticSegment[5]
        {
            HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.5f),
            HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum,0.2f),
            HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.5f),
            HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.rise,0.3f),
            HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.minimum,0.3f)
        };

        snake.m_loop = true;

        //Water snake is a sine wave but with the signature snake spike at the end
        HapticPreset waterSnake = HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.sine, 1.1f, true);
        waterSnake.m_segments[4] = HapticPreset.CreateDefaultSegment(HapticPreset.SegmentType.maximum, 0.1f);

        waterSnake.m_loop = true;

        //Save our custom presets to be used by the runners later
        buriedObjectProperties = new HapticPreset[]
        {
                nothing,
                water,
                snake,
                waterSnake
        };
    }

    //Check dot product of hand vs ideal rotation toward target
    //Turn up intensity of preset runners depending on dot calculation
    void HandleIntensities() {
        if (positionAndRunners.Length == 0) {
            Debug.Log("No runners to alter");
            return;
        }

#if EIR_INTERACTION
        bool isLeft = true;

        //Read the input. If there is no input, all runners for that hand should be zero
        for (int i = 0; i < 2; i++) {
            if (EIRManager.Instance.Interaction.XrBridge.ReadButton(isLeft, ControllerButtons.grip)) {
                AlterIntensities(isLeft);
            }
            else
                AlterIntensities(isLeft, true);

            isLeft = false;
        }



        //Calculate the dot product for each point and turn up the intensities depending on that
        void AlterIntensities(bool left, bool allZero = false) {
            for (int i = 0; i < positionAndRunners.Length; i++) {
                HapticPresetRunner runner;
                GameObject hand;

                if (left) {
                    runner = positionAndRunners[i].Item2;
                    hand = lHand;
                }
                else {
                    runner = positionAndRunners[i].Item3;
                    hand = rHand;
                }

                if (allZero) {
                    runner.m_intensityMultiplier = 0;
                    continue;
                }

                float dot = Vector3.Dot(hand.transform.forward, (positionAndRunners[i].Item1 - hand.transform.position).normalized);

                dot = Mathf.Clamp(dot, 0, 1);

                Debug.Log("Dot: " + dot + " of rotation " + hand.transform.forward + " and " + (positionAndRunners[i].Item1 - hand.transform.position).normalized);

                if (dot > 0.9)
                    runner.m_intensityMultiplier = dot;
                else if (dot == 0)
                    runner.m_intensityMultiplier = 0;
                else if (dot < 0.9)
                    runner.m_intensityMultiplier = dot / 10;

            }
        }
#endif
    }

    //Create all the tuples that contain information needed to alter the intensities
    //Each of the tuples has 2 preset runners assigned to it, one for left and one for right.
    //This means they can be altered independently and thus you can feel different spots with each hand
    void CreateRunners(Vector3[] positions, List<BuriedObjects> buried) {
        positionAndRunners = new (Vector3, HapticPresetRunner, HapticPresetRunner)[buried.Count];
#if EIR_HAPTICS

        for (int i = 0; i < buried.Count; i++) {
            HapticPresetRunner runner1 = EIRManager.Instance.Haptics.CreateHapticPresetRunner(BodyPart.leftHand, buriedObjectProperties[(int)buried[i]]);
            HapticPresetRunner runner2 = EIRManager.Instance.Haptics.CreateHapticPresetRunner(BodyPart.rightHand, buriedObjectProperties[(int)buried[i]]);
            runner1.m_intensityMultiplier = 0;
            runner2.m_intensityMultiplier = 0;

            (Vector3, HapticPresetRunner, HapticPresetRunner) newRunner = (positions[i], runner1, runner2);
            positionAndRunners[i] = newRunner;

        }
#endif

    }

    #endregion

    #region Game logic

    //Select a round based on the number of rounds already completed
    void SelectRound() {
        List<BuriedObjects> buried = new List<BuriedObjects>();
        bool isTutorial = false;

        switch (roundCount) {
            case 0: {
                    text.SetText("Your goal is to find water under the sand! " +
                                "-n-nPoint your hand at the signs and press the grip button on your controller to feel vibrations under the sand" +
                                "-n-n Try it out now so you know what to look for" +
                                "-n-nLook for water and not snakes");
                    text.SetText(text.text.Replace("-n", "\n"));
                    isTutorial = true;
                    buried = new List<BuriedObjects>() { BuriedObjects.water, BuriedObjects.nothing, BuriedObjects.snake };
                    break;
                }
            case 1: {
                    buried = new List<BuriedObjects>() { BuriedObjects.water, BuriedObjects.water, BuriedObjects.nothing };
                    break;
                }
            case 2: {
                    buried = new List<BuriedObjects>() { BuriedObjects.water, BuriedObjects.water, BuriedObjects.snake };
                    break;
                }
            case 3: {
                    buried = new List<BuriedObjects>() { BuriedObjects.water, BuriedObjects.nothing, BuriedObjects.snake };
                    break;
                }
            case 4: {
                    text.SetText("Be careful! There are water snakes in this area! -n-n Take note so you know what to feel for! -n-n More spots have been added too! Check your left and right ");
                    text.SetText(text.text.Replace("-n", "\n"));
                    isTutorial = true;
                    buried = new List<BuriedObjects>() { BuriedObjects.waterSnake };
                    break;
                }
            case 5: {
                    buried = new List<BuriedObjects>() { BuriedObjects.water, BuriedObjects.waterSnake, BuriedObjects.waterSnake, BuriedObjects.snake };
                    break;
                }
            case 6: {
                    buried = new List<BuriedObjects>() { BuriedObjects.water, BuriedObjects.water, BuriedObjects.waterSnake, BuriedObjects.waterSnake };
                    break;
                }
            case 7: {
                    buried = new List<BuriedObjects>() { BuriedObjects.water, BuriedObjects.water, BuriedObjects.nothing, BuriedObjects.waterSnake };
                    break;
                }
            case 8: {
                    buried = new List<BuriedObjects>() { BuriedObjects.water, BuriedObjects.waterSnake, BuriedObjects.snake, BuriedObjects.snake, BuriedObjects.nothing };
                    break;
                }
            case 9: {
                    buried = new List<BuriedObjects>() { BuriedObjects.water, BuriedObjects.waterSnake, BuriedObjects.waterSnake, BuriedObjects.snake, BuriedObjects.snake };
                    break;
                }
            case 10: {
                    buried = new List<BuriedObjects>() { BuriedObjects.nothing, BuriedObjects.waterSnake, BuriedObjects.waterSnake, BuriedObjects.waterSnake, BuriedObjects.waterSnake };
                    break;
                }
            case 11: {
                    buried = new List<BuriedObjects>() { BuriedObjects.nothing, BuriedObjects.snake, BuriedObjects.water, BuriedObjects.waterSnake };
                    text.SetText("Great job! Your final score was " + score + "! -n-n Use the panel to the right to return to the Test Room");
                    text.SetText(text.text.Replace("-n", "\n"));
                    isTutorial = true;
                    break;
                }
        }

        SetupRound(buried, isTutorial);

    }

    //Setup the buried objects and signs and buttons
    void SetupRound(List<BuriedObjects> buried, bool isTutorial) {
        if (buried.Count > objectPositions.Length) {
            Debug.LogError("Not enough positions");
            return;
        }

        if (!isTutorial) {
            buried = RandomiseList(buried);
        }

        CreateRunners(objectPositions, buried);
        CreateAndRegisterSignsAndButtons(objectPositions, buried);

        if (isTutorial) {
            for (int i = 0; i < buried.Count; i++) {
                ShowCorrectOnSign(i);
            }

            DestroyChoiceButtons();

            if (roundCount != 11)
                nextStageButton.SetActive(true);

        }
    }

    List<BuriedObjects> RandomiseList(List<BuriedObjects> list) {
        System.Random rand = new System.Random();

        return list.OrderBy(a => rand.Next()).ToList();
    }

    //Create signs and buttons
    void CreateAndRegisterSignsAndButtons(Vector3[] positions, List<BuriedObjects> buried) {
        for (int i = 0; i < buried.Count; i++) {
            GameObject signObject = Instantiate(signPrefab, positions[i], Quaternion.identity);
            signObject.transform.LookAt(new Vector3(0, 4.5f, 0));
            signObject.transform.Rotate(new Vector3(0, 180, 0));
            activeSigns.Add(signObject);

            GameObject button = Instantiate(buttonPrefab, positions[i] + Vector3.up * 1.5f, Quaternion.identity);
            button.transform.LookAt(new Vector3(0, 6f, 0));
            button.transform.Rotate(new Vector3(0, 180, 0));

            AddButtonListener(i, button);

            activeButtons.Add(button);
            button.transform.SetParent(canvas.transform);

        }


        activeBuriedObjects = buried;

        void AddButtonListener(int index, GameObject button) {
            button.GetComponent<Button>().onClick.AddListener(delegate { CheckGuess(index); });
        }
    }

    //Check if your guess was correct and assign points and end the round
    public void CheckGuess(int spot) {
        switch (activeBuriedObjects[spot]) {
            case BuriedObjects.nothing: {
                    text.SetText("You found...... nothing");
                    break;
                }
            case BuriedObjects.water: {
                    text.SetText("You found...... water! +100 points");
                    score += 100;
                    break;
                }
            case BuriedObjects.snake: {
                    text.SetText("You found...... a snake! -100 points");
                    score -= 100;
                    break;
                }
            case BuriedObjects.waterSnake: {
                    text.SetText("You found...... a water snake! -100 points");
                    score -= 100;
                    break;
                }
        }

        UpdateScore();

        DestroyChoiceButtons();

        nextStageButton.SetActive(true);

        for (int i = 0; i < activeBuriedObjects.Count; i++) {
            ShowCorrectOnSign(i);
        }
    }

    //Makes a sign show the correct sprite
    void ShowCorrectOnSign(int index) {
        activeSigns[index].GetComponentInChildren<SpriteRenderer>().sprite = signSprites[(int)activeBuriedObjects[index]];
    }

    void UpdateScore() {
        scoreText.text = score.ToString();
    }

    void DestroyChoiceButtons() {
        for (int i = 0; i < activeButtons.Count; i++) {
            Destroy(activeButtons[i]);
        }

        activeButtons.Clear();
    }


    public void RoundComplete() {
        ClearUpAll();
        roundCount++;

        nextStageButton.SetActive(false);

        SelectRound();
    }

    void ClearUpAll() {
        for (int i = 0; i < activeBuriedObjects.Count; i++) {
            Destroy(activeSigns[i]);
        }

        DestroyChoiceButtons();

        positionAndRunners = new (Vector3, HapticPresetRunner, HapticPresetRunner)[0];
        activeBuriedObjects.Clear();
        activeButtons.Clear();
        activeSigns.Clear();


        text.SetText("");
    }

    #endregion
    */
}