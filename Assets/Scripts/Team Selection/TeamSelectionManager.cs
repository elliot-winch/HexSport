using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionManager : MonoBehaviour {

	public GameObject selectionButtonPrefab; 
	public Canvas mainCanvas;
	public int teamSize;

	List<ContestantData> neutralContestants;

	List<Team> teams;
	int currentTeamIndex;

	void Start () {

		//temp spawn in brand new contestants each time
		teams = new List<Team>();

		teams.Add (new Team ("Blittering Broncos", new Color (1f, 165f / 256f, 0f)));
		teams.Add (new Team ("Church of Sixty Suns", new Color (139f / 256f, 0f, 0139f / 256f)));


		string[] names = new string[] {
			"Andre", "Bucky", "Chris", "Danny",	"Eddy", "Frank", "Gummy","Harold","Illy", "James"
		};

		neutralContestants = new List<ContestantData> ();
		for(int i = 0; i < teamSize * 2; i++) {
			neutralContestants.Add (new ContestantData (names[i], Random.value * 10, Random.value * 10, Random.value * 10, 10));

			GameObject button = Instantiate(selectionButtonPrefab, mainCanvas.transform);

			button.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0.5f, 1f);
			button.GetComponent<RectTransform>().localPosition = new Vector3(0f, - i * (button.GetComponent<RectTransform>().rect.height * 0.5f) ,0f);

			Button.ButtonClickedEvent bcevent = new Button.ButtonClickedEvent ();

			bcevent.AddListener( () => {
				Debug.Log("PooP");
			});

			button.GetComponent<Button>().onClick = bcevent;
			button.GetComponentInChildren<Text> ().text = names [i];
		}

	}

}
