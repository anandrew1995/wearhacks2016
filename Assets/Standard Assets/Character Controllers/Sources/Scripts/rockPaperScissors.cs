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
	public Text scoreText;
	public GameObject heart;
	public Text counterText;
	private GameObject computerChoice = null;
	private int counter = 0;
	private bool countDone = false;
	public bool playerHand = false;
	public bool playerRdy = false;
	private string rps;
	public float score = 0;
	private bool hasCompPlayed = false;
	private bool isPlaying = false;
	private int result;
	private bool initialHeart = true;
	private bool boxChosen = false;
	//private string computerChoiceString;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		mainCanvas.transform.rotation = mainCamera.transform.rotation;
		mainCanvas.transform.position = new Vector3 (mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
		if (playerHand) {
			float computerHand = Random.Range(0f, 0.99f);

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
			if(rps.Equals("rock") && !boxChosen) {
				boxChoice = rockBox;
				boxChosen = true;
				rockBox.transform.parent = mainCanvas.transform;
				if (computerChoice == rock){
					Debug.Log ("Tie");
					result = 1;
				}else if(computerChoice == paper){
					Debug.Log ("Lose");
					result = 0;
				}else if(computerChoice == scissor){
					score++;
					Debug.Log ("win by rock");
					result = 2;
				}
			}

			if(rps.Equals("scissors") && !boxChosen) {
				boxChoice = scissorBox;
				boxChosen = true;
				scissorBox.transform.parent = mainCanvas.transform;
				if (computerChoice == rock){
					Debug.Log ("Lose");
					result = 0;
				}else if(computerChoice == paper){
					score++;
					Debug.Log ("win by scissor");
					result = 2;
				}else if(computerChoice == scissor){
					Debug.Log ("Tie");
					result = 1;
				}
			}

			if(rps.Equals("paper") && !boxChosen) {
				boxChoice = paperBox;
				boxChosen = true;
				paperBox.transform.parent = mainCanvas.transform;
				if (computerChoice == rock){
							score++;
							Debug.Log ("win by paper");
					result = 2;
				}else if(computerChoice == paper){
					Debug.Log ("Tie");
					result = 1;
				}else if(computerChoice == scissor){
					Debug.Log ("Lose");
					result = 0;
				}
			}
					if (result == 2) {

			heart.transform.parent = canvas.transform;
			if (initialHeart) {
				heart.transform.position = new Vector3 (transform.position.x, transform.position.y + 1f, transform.position.z);
				initialHeart = false;
			}
			heart.transform.Rotate(Vector3.up * 20 * Time.deltaTime);
				heart.transform.Translate (Vector3.up  * (Time.deltaTime/200));
			heart.transform.localScale = new Vector3(heart.transform.localScale.x * 1.05f,heart.transform.localScale.y * 1.05f,heart.transform.localScale.z * 1.05f);

					}

			boxChoice.transform.position = new Vector3 (mainCanvas.transform.position.x+1.0f, mainCanvas.transform.position.y, mainCanvas.transform.position.z+0.5f);
			computerChoice.transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
			computerChoice.transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
			counterText.transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
			counterText.transform.position = new Vector3(transform.position.x, transform.position.y + 1.25f, transform.position.z);
			scoreText.text = score.ToString();
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
		//counter --;
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
		counterText.text = "";
		rock.transform.parent = null;
		paper.transform.parent = null;
		scissor.transform.parent = null;
		rockBox.transform.parent = null;
		paperBox.transform.parent = null;
		scissorBox.transform.parent = null;
		heart.transform.parent = null;
		heart.transform.position = new Vector3 (500, 500, 500);
		hasCompPlayed = false;
		isPlaying = false;
		playerHand = false;
		playerRdy = false;
		initialHeart = true;
		boxChosen = false;
	}

	public void setPlayerHand(string hand){
		rps = hand;
	}

	public float getScore() {
		return score;
	}
}
