using UnityEngine;
using System.Collections;

public class ButtonPressing : MonoBehaviour {

    SequenceInfo sequence;
    bool printed = false;

	// Use this for initialization
	void Start () {
        sequence.pressLength = 4;
        sequence.pressProgress = 0;
	    RandButtonChain(sequence.pressLength);
	}
	
	// Update is called once per frame
	void Update () {

        if (sequence.pressProgress == 0) {
            CheckInput(sequence.buttonChain.Substring(0,1));
        } else if (sequence.pressProgress == 1) {
            CheckInput(sequence.buttonChain.Substring(1,1));
        } else if (sequence.pressProgress == 2) {
            CheckInput(sequence.buttonChain.Substring(2,1));
        } else if (sequence.pressProgress == 3) {
            CheckInput(sequence.buttonChain.Substring(3,1));
        }

        if (sequence.pressProgress == sequence.pressLength &&
                printed == false) {
            print("sequence completed!!!");
            printed = true;
        }

    }

    void RandButtonChain(int n) {

        int done = 0;
        // 1 = up, 2 = down, 3 = left, 4 = right
        System.Random rnd = new System.Random();

        for (int i=0; i<n; i++) {
            int gen = rnd.Next(1,5); // does not include MaxValue

            sequence.buttonChain = sequence.buttonChain + gen.ToString();
        }
        BuildRequiredPressSequence(n);
    }

    void BuildRequiredPressSequence(int n) {
        for (int i=0; i<n; i++) {
            if (sequence.buttonChain.Substring(i,1) == "1"){
                print("UP");
            } else if (sequence.buttonChain.Substring(i,1) == "2") {
                print("DOWN");
            } else if (sequence.buttonChain.Substring(i,1) == "3") {
                print("LEFT");
            } else if (sequence.buttonChain.Substring(i,1) == "4") {
                print("RIGHT");
            } 
        }
    }

    void CheckInput(string s) {
        if (Input.GetKeyDown(ConvertIndexToKey(s))) {
            print(s + " key pressed");
            sequence.pressProgress += 1;
        }
    }

    string ConvertIndexToKey(string s) {
        if (s == "1") {
            return "w";
        }
        if (s == "2") {
            return "s";
        }
        if (s == "3") {
            return "a";
        }
        if (s == "4") {
            return "d";
        }
        return "0";
    }


    void CheckRequiredPressSequence() {
        if (Input.GetKeyDown(KeyCode.W)) {
            print("w pressed, complete = true");
            sequence.complete = true;
        }
    }

    struct SequenceInfo {
        public bool complete;
        public string buttonChain;
        public int pressLength;
        public int pressProgress;
    }

}
