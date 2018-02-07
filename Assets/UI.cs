﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameState;

public class UI : MonoBehaviour {

	// Use this for initialization
	
	//The current player giving input
	Player activePlayer;
	//Cards to display
	GameObject[] currButtons;
	GameObject[] cardsToShow;
	//public Card inputCard;
	GameManager gm;
	

	state gameState = state.STANDBY;
	
	Card[] multipleCardInput;
	
	GameObject instructionHeader;  //This gives th current instruction to the player
	GameObject headerCurrPlayer; //This says which player's turn it is
	GameObject messageHeader; //Gives any messages to the player
	
	GameObject cardCenter;
	
	GameObject enemyBP;
	GameObject playerBP;
	Vector2 canvasSize;
	
	GameObject canvas;
	
	//Use these variables to place this on UI, this assures that the relative positions will change the same in different resolutions
	float panelWidth;
	float panelHeight;
	float panelPosX;
	float panelPosY;
	
	public UI(GameManager _gm) {
		
		canvas = GameObject.Find("Canvas");
		panelWidth = canvas.GetComponent<RectTransform>().rect.width * canvas.GetComponent<RectTransform>().localScale.x;
		panelHeight = canvas.GetComponent<RectTransform>().rect.height * canvas.GetComponent<RectTransform>().localScale.y;
		panelPosX = canvas.GetComponent<RectTransform>().position.x;
		panelPosY = canvas.GetComponent<RectTransform>().position.y;
		gm = _gm;
		
		instructionHeader  = createHeaderMessage(panelPosX, panelPosY + panelHeight/4, "Current action required");	
		headerCurrPlayer  = createHeaderMessage(panelPosX, panelPosY + panelHeight/5, "Current player's turn");	
		messageHeader  = createHeaderMessage(panelPosX, panelPosY - panelHeight/2, " ");	
	}
	
	void Start () { }
	
	// Update is called once per frame
	void Update () {
	}
	
	//Prints out a given hand
	public GameObject[] showHand(Card[] hand){
		int n = hand.Length;
		if(cardsToShow != null)
		{
			for(int i = 0; i < cardsToShow.Length; i ++) {
				Destroy(cardsToShow[i]);
				cardsToShow[i] = null;
			}
		}
		cardsToShow = new GameObject[n];
		float buffer = panelWidth/(n*2);
		float cardWidth = panelWidth/15;
		float offsetX = (panelWidth - n*buffer)/6;
		for(int i = 0; i< n; i++)
		{	
			Vector2 pos = 	new Vector2(panelPosX - (buffer+cardWidth)*(n/2) + offsetX + i*buffer, panelPosY -  panelHeight/6);
			cardsToShow[i] = (GameObject)Instantiate(Resources.Load("UICardButton"), pos , Quaternion.identity);			
			cardsToShow[i].GetComponent<CardButtonUI>().init(hand[i], this, pos);
		}
		return cardsToShow;
	}	
	
	//Ask player for input
	public void gotCardSelection(Card selected){
		/*
		This method is called when a card is clicked. Depending on the current gameState, a difference received mthod will be called.
		*/
		
		if (gameState == state.ASKINGFORSTAGES) {
			gotStageSelection(selected);
		}
		if (gameState == state.ASKINGFORCARDSINQUEST) {
			gotBattleCardSelection(selected);
		}
	}
	public void gotButtonClick(string input) {
		//This method is called when a button is clicked
		if(gameState == state.ASKINGFORSPONSORS) { //If the game is current looking for sponsors
			if(input.Equals("Yes")) { //If the current player wants to be sponsor
				gameState = state.STANDBY; 
				clearButtons();
				gm.startQuestSetup(); //Tell GameManager to set the current player as sponsor
			}
			else {
				gm.getSponsor(); //Other wise have GameManager call getSponsor for the next player.
			}
		}
		else if(gameState == state.ASKINGFORPLAYERS){
			if(input.Equals("Yes")) { //If the current player wants to be sponsor 
				gameState = state.STANDBY; 
				clearButtons();
				gm.gotPlayer(activePlayer); //Tell GameManager to set the current player as sponsor
			}
			else {
				clearButtons();
				gm.gotPlayer(null); //Other wise have GameManager call getSponsor for the next player.
			}
		}
		else if(gameState == state.ASKINGFORCARDSINQUEST){
			if(input.Equals("FIGHT")) {
				gm.questAttack(cleanUpArray(multipleCardInput));
			}
			else if(input.Equals("Give up")) {
				gm.forfeitQuest();
			}
		}
		else if(gameState == state.ASKINGFORSTAGES) {
			clearButtons();
			clearDeckOnScreen();
			gm.endQuest("Quest forfeited");
			displayAlert("Quest forfeited");
		}
	}

	public void askForBattleCardSelection(Player player){
		
		activePlayer = player;
		cardsToShow = showHand(getOnlyTypeFromDeck(player.getHand(), false, true, true)); //Display the cards
		gameState = state.ASKINGFORCARDSINQUEST;
		multipleCardInput = new Card[player.getHand().Length]; //Get multipleCardInput ready to hold the new card choices
		changeHeaderMessage("Select cards to play, then press FIGHT", instructionHeader);
		return;
	}
	
	private void gotBattleCardSelection(Card selected){
		for(int i = 0; i < multipleCardInput.Length; i++) {  //Find the next empty spot in multipleCardInput
			if(multipleCardInput[i] == null) { 
				multipleCardInput[i] = selected; //Add the new selected card to multipleCardInput
				break;
			}
		}
		int extraBP = 0;
		for(int i = 0; i< multipleCardInput.Length; i++)
		{
			if(multipleCardInput[i] == null){break;}
			extraBP = extraBP + multipleCardInput[i].getBP();
		}
		changeHeaderMessage("Player BP: " + (activePlayer.getBP() + extraBP), playerBP);
	}
	
	public void askForStageSelection(Player player, int n){
		Debug.Log("askForStageSelection");
		activePlayer = player;
		Card [] foesOnly = getOnlyTypeFromDeck(player.getHand(), true, false, false);
		cardsToShow = showHand(foesOnly); //Display the cards
		gameState = state.ASKINGFORSTAGES;
		multipleCardInput = new Card[n]; //Get multipleCardInput ready to hold the new card choices
		Debug.Log("created new multipleCardInput: " + multipleCardInput.Length );
		changeHeaderMessage("Select card 1 out of " + multipleCardInput.Length, instructionHeader);
		createButtonMessage(panelPosX - panelWidth/5, panelPosY + panelHeight/5, "Forfeit");
		return;
	}
	public void drawingQuestCard()
	{
		clearDeckOnScreen();
		clearButtons();
		changeHeaderMessage("Drawing new card", instructionHeader);
		changeHeaderMessage(" ", messageHeader);
		changeHeaderMessage(" ", headerCurrPlayer);
		
	}
	private void gotStageSelection(Card selected){
		Debug.Log("gotStageSelection: " + multipleCardInput.Length );
		for(int i = 0; i < multipleCardInput.Length; i++) {  //Find the next empty spot in multipleCardInput
			if(multipleCardInput[i] == null) { 
				multipleCardInput[i] = selected; //Add the new selected card to multipleCardInput
				if(i == multipleCardInput.Length-1) {  //If all the cards has been chosen
					
					changeHeaderMessage("Stages selected", instructionHeader); //Update header
					gameState = state.STANDBY;
					clearDeckOnScreen();
					gm.endQuestSetup(multipleCardInput); //Send cards back to GameManager
					
					return;
				}
				changeHeaderMessage("Select card " + (i+2) + " out of " + multipleCardInput.Length, instructionHeader);
				return;
			}
		}
	}

	public void askYesOrNo(Player player, string message, state messageState) {
		/*
		This method creates two buttons, yes or no. When one of these are clicked, gotButtonClick will be called and
		will have the appropriate action done according to the current state.
		*/
		clearButtons();
		gameState = messageState;
		activePlayer = player;
		
		changeHeaderMessage(activePlayer.getName() + "'s turn", headerCurrPlayer);	
		changeHeaderMessage(message, instructionHeader);
		
		//Display yes or no buttons
		createButtonMessage(panelPosX - panelWidth/5, panelPosY + panelHeight/5, "Yes");
		createButtonMessage(panelPosX + panelWidth/5, panelPosY + panelHeight/5, "No");
	}
	
	//UI clean up
	private void clearButtons(){
		//Clears any buttons on screen
		if(currButtons == null)
		{
			Debug.Log("No buttons to clear");
			return;
		}
		for(int i = 0; i < currButtons.Length; i ++) {
			Debug.Log("Deleting...");
			Destroy(currButtons[i]);
		}
		
		currButtons = null;
	}
	
	private void clearDeckOnScreen(){
		//Clears the selectable deck of cards on screen
		if(cardsToShow == null)
		{
			return;
		}
		for(int i = 0; i < cardsToShow.Length; i ++) {
			Destroy(cardsToShow[i]);
		}
		cardsToShow = null;
	}

	//Creating and modifying headers and buttons
	private GameObject createButtonMessage(float x, float y, string newText = "Button") {
		GameObject tempButton = (GameObject)Instantiate(Resources.Load("UIButton"), new Vector2(x, y), Quaternion.identity);			
		tempButton.GetComponent<ButtonUI>().init(this);
		tempButton.GetComponentInChildren<Text>().text = newText;
		Debug.Log("createButtonMessage");
		int n = 0;
		if(currButtons != null) {
			
			n = currButtons.Length;
		}
		
			GameObject[] temp = new GameObject[n+1];
			for(int i = 0; i< n; i++)
			{
				temp[i] = currButtons[i];
			}
			temp[n] = tempButton;
			currButtons = temp;
		return tempButton;
	}
	
	private GameObject createHeaderMessage(float x, float y, string input = "Header") {
		GameObject header;
		header = (GameObject)Instantiate(Resources.Load("UIHeader"), new Vector2(x, y), Quaternion.identity);	
		header.GetComponent<HeaderUI>().init();
		changeHeaderMessage(input, header);
		return header;
	}
	
	private void changeHeaderMessage(string input, GameObject header){
		header.GetComponent<Text>().text = input;
	}
	
	public void showCard(Card cardToShow){
		if(cardCenter != null) { Destroy(cardCenter); }
		//cardCenter = (GameObject)Instantiate(Resources.Load("UICard"), new Vector2((float)(panelWidth/3.5), (float)(panelHeight/2.5)), Quaternion.identity);	
		
		cardCenter = (GameObject)Instantiate(Resources.Load("UICard"), new Vector2(panelPosX - panelWidth/20, panelPosY + panelHeight/4), Quaternion.identity);			
		//cardCenter.GetComponent<CardButtonUI>().setCard(cardToShow);
		cardCenter.GetComponent<CardButtonUI>().init(cardToShow, this, new Vector2(panelPosX - panelWidth/20, panelPosY + panelHeight/4));
	}

	public void showStage(ActiveQuest activeQuest){
		clearDeckOnScreen();
		clearButtons();
		if(activeQuest.getQuest() == null)
		{
			gameState = state.STANDBY;
			clearDeckOnScreen();
		}
			
		if(playerBP == null) { playerBP = createHeaderMessage(panelWidth/4, panelHeight/2, " ");}
		if(enemyBP == null) { enemyBP = createHeaderMessage(panelWidth - panelWidth/4, panelHeight/2, " ");}
		
		showCard(activeQuest.getCurrentStage());
		activePlayer = activeQuest.getCurrentPlayer();
		changeHeaderMessage("Player BP: " + activePlayer.getBP(), playerBP);
		changeHeaderMessage("Enemy BP: " + activeQuest.getCurrentStage().getBP(), enemyBP);
		changeHeaderMessage(activePlayer.getName() + "'s turn", headerCurrPlayer);	
		changeHeaderMessage("Select the cards you wish to play to defeat this foe.", instructionHeader);	
		createButtonMessage(panelPosX, panelPosY - panelHeight/10, "FIGHT");
		createButtonMessage(panelPosX - panelWidth/5, panelPosY + panelHeight/5, "Give up");
		gameState = state.ASKINGFORCARDSINQUEST;

	}
	
	//Other utilities
	public void displayAlert(string input)
	{
		changeHeaderMessage(input, messageHeader);
	}
	private Card[] cleanUpArray(Card [] oldArr){
		int newN = 0;
		for(int i = 0; i< oldArr.Length; i++)
		{
			if(oldArr[i] == null)
			{
				newN = i;
				break;
			}
		}
		Card [] newArr = new Card[newN];
		for(int i = 0; i < newArr.Length; i++)
		{
			newArr[i] = oldArr[i];
		}
		return newArr;
		
	}
	
	private Card[] getOnlyTypeFromDeck(Card[] deck, bool getFoes, bool getWeap, bool getAlly){
		Card[] tempHand = new Card[deck.Length];
		int counter = 0;
		for(int i = 0; i < deck.Length; i++ )
		{
			if(Object.ReferenceEquals(deck[i].GetType(), typeof(Foe)) && getFoes)
			{
				tempHand[counter] = deck[i];
				counter++;
			}
			else if(Object.ReferenceEquals(deck[i].GetType(), typeof(Weapon)) && getWeap)
			{
				tempHand[counter] = deck[i];
				counter++;
			}
			else if(Object.ReferenceEquals(deck[i].GetType(), typeof(Ally)) && getAlly)
			{
				tempHand[counter] = deck[i];
				counter++;
			}
		}
		
		Card[] newTempHand = new Card[counter];
		
		for(int i = 0; i < counter; i++)
		{
			newTempHand[i] = tempHand[i];
		}
		return newTempHand;
	}
	
	public void endQuest()
	{
		clearButtons();
		Destroy(enemyBP);
		Destroy(playerBP);
		
		enemyBP = null;
		playerBP = null;
	}
}