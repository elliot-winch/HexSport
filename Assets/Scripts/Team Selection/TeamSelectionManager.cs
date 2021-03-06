using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TeamSelectionManager : MonoBehaviour {

	public GameObject selectionButtonPrefab; 
	public Canvas mainCanvas;
	public int teamSize;
	public Sprite[] teamLogos;

	Dictionary<Team, Transform> holder;

	int currentTeamIndex;
	int numSelected = 0;

	void Start () {

		//generalise
		Team t1 = new Team ("Broncos", new Color (1f, 165f / 256f, 0f), teamLogos[0]);
		Team t2 = new Team ("Raiders", new Color (139f / 256f, 0f, 0139f / 256f), teamLogos[1]);

		//temp spawn in brand new contestants each time
		//teamPositions = new Dictionary<Team, Vector3> ();
		holder = new Dictionary<Team, Transform> ();

		//teamPositions[t1] = new Vector3(0.6f * selectionButtonPrefab.GetComponent<RectTransform>().rect.width, 0f);
		//teamPositions[t2] = new Vector3(Screen.width - (0.6f * selectionButtonPrefab.GetComponent<RectTransform>().rect.width),  0f);

		holder [t1] = mainCanvas.transform.Find ("Left Team Canvas");
		holder [t2] = mainCanvas.transform.Find ("Right Team Canvas");

		//Contestant Data has three stats. Do we want ContestantData.NumberStats (static) or only know about it here?
		//also text field needs to be 20 height, should probably encapsulate
		float buttonHeight = (5 * 20);
		int numCols = Mathf.CeilToInt(((buttonHeight * 0.95f) * teamSize * 2) / (Screen.height - 160));
		int numRows = Mathf.CeilToInt ((teamSize * 2f) / numCols);

		Font arial = Resources.GetBuiltinResource (typeof(Font), "Arial.ttf") as Font;

		string[] names = new string[] {
			"Andre", "Bucky", "Chris", "Danny",	"Eddy", "Frank", "Gummy","Harold", "Illy", "James"
		};
			
		for(int i = 0; i < numRows; i++) {
			for(int j = 0; j < numCols; j++) {

				if (i * numCols + j >= (teamSize * 2)) {
					return;
				}

				ContestantData cd = new ContestantData (names [i * numCols + j], (int)(Random.value * 10), (int)(Random.value * 10), (int)(Random.value * 10), 10);

				GameObject button = Instantiate(selectionButtonPrefab, mainCanvas.transform);

				RectTransform bRT = button.GetComponent<RectTransform> ();

				bRT.sizeDelta = new Vector2 (bRT.rect.width, buttonHeight);
				bRT.localPosition = new Vector3((i - ((numRows - 1) / 2f)) * (bRT.rect.width * 1.05f), (Screen.height / 2) - 80 - j * (bRT.rect.height * 1.05f) ,0f);
				DisplayStats (button, cd.Stats, arial);

				button.GetComponent<Button>().onClick = CreateOnButtonPressed(button.GetComponent<Button>(), cd);
			}
		}
		//potentially unreachable code
	}

	Button.ButtonClickedEvent CreateOnButtonPressed(Button b, ContestantData cd){

		Button.ButtonClickedEvent bcevent = new Button.ButtonClickedEvent ();

		bcevent.AddListener( () => {
			b.interactable = false;

			Team t = holder.Keys.ToList()[currentTeamIndex];
			t.AddContestant(cd);

			b.transform.SetParent(holder[t], worldPositionStays: true);
			b.transform.localPosition = new Vector3(0f, -t.Contestants.Count * (b.GetComponent<RectTransform>().rect.height * 0.25f));

			b.GetComponent<TeamSelectorButton>().originalChildPosition = b.transform.GetSiblingIndex ();
			b.GetComponent<TeamSelectorButton>().active = true;

			ColorBlock cb = b.colors;
			cb.disabledColor = t.Color;
			b.colors = cb;

			CheckEndOfList();
		});

		return bcevent;
	}

	void CheckEndOfList(){

		currentTeamIndex = (currentTeamIndex + 1) % holder.Keys.Count;

		numSelected++;

		if (numSelected >= teamSize * 2) {
			GameObject transferer = new GameObject ();

			transferer.name = "Team Selection Data";
			TeamTransfer tt = transferer.AddComponent<TeamTransfer> ();
			tt.Teams = holder.Keys.ToList ();

			DontDestroyOnLoad (transferer);

			SceneManager.LoadScene ("Main");
		}
	}


	void DisplayStats(GameObject p, Dictionary<string, string> data, Font font){

		float startY = p.transform.position.y + 50; //hacky
		float counter = 1;
		foreach (KeyValuePair<string, string> kv in data) {
			GameObject textObj = new GameObject ();
			textObj.transform.parent = p.transform;

			Text t = textObj.AddComponent<Text> ();
			t.text = string.Format ("{0}: {1}", kv.Key, kv.Value);
			t.font = font;
			t.GetComponent<RectTransform> ().sizeDelta = new Vector2 (120, 20);
			t.alignment = TextAnchor.MiddleCenter;
			t.fontSize = 14;
			t.color = Color.black;

			textObj.transform.position = new Vector3 (p.transform.position.x, startY - (counter * textObj.GetComponent<RectTransform>().rect.height));

			counter++;
		}
	}
}
