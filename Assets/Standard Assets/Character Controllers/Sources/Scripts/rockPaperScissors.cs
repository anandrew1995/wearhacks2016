using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class rockPaperScissors : MonoBehaviour {

	public GameObject canvas;
	public GameObject rock;
	public GameObject paper;
	public GameObject scissor;
	public GameObject rockBox;
	public GameObject paperBox;
	public GameObject scissorBox;
	private GameObject boxChoice;
	public GameObject mainCanvas;
	public GameObject mainCamera;
	public Text counterText;
	private GameObject computerChoice = null;
	private int counter = 0;
	private bool countDone = false;
	public bool playerHand = false;
	public bool playerRdy = false;
	private string rps;
	public float score;
	private bool hasCompPlayed = false;
	private bool isPlaying = false;
	//private string computerChoiceString;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		mainCanvas.transform.rotation = mainCamera.transform.rotation;
		mainCanvas.transform.position = new Vector3 (mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
		if (playerHand) {
			float computerHand = Random.Range(0f, 3f);

			if (!hasCompPlayed && isPlaying) {
				if (computerHand < 1f) {
					//rock
					rock.transform.parent = canvas.transform; 
					computerChoice = rock;
					hasCompPlayed = true;
					//playerRdy = false;
					//	computerChoiceString = "rock";
				} else if (computerHand < 2f) {
					//paper
					paper.transform.parent = canvas.transform;
					computerChoice = paper;
					hasCompPlayed = true;
					//playerRdy = false;
					//	computerChoiceString = "paper";
				} else if (computerHand <= 3f) {
					//scissors
					scissor.transform.parent = canvas.transform;
					computerChoice = scissor;
					hasCompPlayed = true;
					//playerRdy = false;
					//	computerChoiceString = "s";
				}
			}
		}
		// canvas.transform.rotation.y = transform.rotation.y;
		if (computerChoice && playerHand) {
			//rps vs computer choice
			if(rps.Equals("rock")) {
				boxChoice = rockBox;
				rockBox.transform.parent = mainCanvas.transform;
				if (computerChoice == rock){
					Debug.Log ("Tie");
				}else if(computerChoice == paper){
					Debug.Log ("Lose");
				}else if(computerChoice == scissor){
					score++;
					Debug.Log ("win by rock");
				}
			}

			if(rps.Equals("scissors")) {
				boxChoice = scissorBox;
				scissorBox.transform.parent = mainCanvas.transform;
				if (computerChoice == rock){
					Debug.Log ("Lose");
				}else if(computerChoice == paper){
						score++;
						Debug.Log ("win by scissor");
				}else if(computerChoice == scissor){
					Debug.Log ("Tie");
				}
			}

			if(rps.Equals("paper")) {
				boxChoice = paperBox;
				paperBox.transform.parent = mainCanvas.transform;
				if (computerChoice == rock){
							score++;
							Debug.Log ("win by paper");
				}else if(computerChoice == paper){
					Debug.Log ("Tie");
				}else if(computerChoice == scissor){
					Debug.Log ("Lose");
				}
			}

			boxChoice.transform.position = Vector3.zero;
			computerChoice.transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
			computerChoice.transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
			counterText.transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
			counterText.transform.position = new Vector3(transform.position.x, transform.position.y + 1.25f, transform.position.z);
		}
	}

	IEnumerator StartCounter()
	{
		counterText.text= counter.ToString();
		Debug.Log (counterText.text);
		counter --;
		yield return new WaitForSeconds(1);
		counterText.text= counter.ToString();
		Debug.Log (counterText.text);
		counter --;
		yield return new WaitForSeconds(1);
		counterText.text= counter.ToString();
		Debug.Log (counterText.text);
		counter --;
		yield return new WaitForSeconds(1);
		playerRdy = true; //accept playerHand
		counterText.text = " ";
	}

	public void Play () {
		//rock, paper, scissor
		counter = 3;
		StartCoroutine(StartCounter());
		isPlaying = true;
	}

	public void DonePlaying() {
		counterText.text = " ";
		rock.transform.parent = null;
		paper.transform.parent = null;
		scissor.transform.parent = null;
		rockBox.transform.parent = null;
		paperBox.transform.parent = null;
		scissorBox.transform.parent = null;
		hasCompPlayed = false;
		isPlaying = false;
		playerHand = false;
		playerRdy = false;
	}

	public void setPlayerHand(string hand){
		rps = hand;
	}
}
