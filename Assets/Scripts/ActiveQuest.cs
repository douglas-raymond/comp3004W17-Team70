﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveQuest{

	QuestCard quest;
	Card [] stages;
	Card [][] stageWeapons;
	Player [] players;
	int playerNum;
	int stageNum;
	Player sponsor;
	int currentStage;
	Player currentPlayer;
	int highestBid;
	Player highestBidder;
	Card [] tentativeBet;
	
	bool inProgress;
	public ActiveQuest(QuestCard _quest) {
		quest = _quest;
		stageNum = _quest.getStages();
		playerNum = 0;
		currentStage = 0;
		highestBid = -1;
		highestBidder = null;
		inProgress = false;
	}
	
	public void addPlayer(Player newPlayer) {
		int n = playerNum;
		Player[] temp = new Player[playerNum+1];
		for(int i = 0; i < playerNum; i++)
		{
			temp[i] = players[i];
		}
		temp[playerNum] = newPlayer;
		
		players = temp;
		playerNum ++;
		currentPlayer = players[0];
	}
	
	public void deletePlayer(Player player) {
		if(playerNum == 1) {
			playerNum = 0;
			currentPlayer = null;
			players = null;
			quest = null;
			return;
		}
		int indexToDelete = getPlayerInt(player);
		if(indexToDelete == -1) {
			Debug.Log("Not found");
			return;
		}
		
		Player [] newArr = new Player[players.Length-1];
		playerNum --;
		if(indexToDelete == players.Length-1) {
			for(int i = 0; i < newArr.Length; i++) {
				newArr[i] = players[i];	
			}
			currentPlayer = newArr[newArr.Length-1];
			nextStage();
		}
		else if(indexToDelete == 0) {
			for(int i = 0; i < newArr.Length; i++) {
				newArr[i] = players[i+1];
			}
			currentPlayer = newArr[0];
		}
		else {
			currentPlayer = players[indexToDelete+1];
			for(int i = 0; i < indexToDelete; i++) {
				newArr[i] = players[i];
			}
			for(int i = indexToDelete + 1; i < players.Length; i++) {
				newArr[i-1] = players[i];
			}
		}
		
		players = newArr;
	}
	private void finishQuest() {
		if(playerNum == 0) { return;}
		for(int i = 0; i< players.Length; i ++)
		{
			players[i].addShields(stageNum);
		}
		quest = null;
		inProgress = false;
	}

	public void nextPlayer() {
		if(playerNum == 0) {
			Debug.Log("Quest lost, No players left");
			quest = null;
		}
		int currentPlayerIndex = getPlayerInt(currentPlayer);

		if(currentPlayerIndex == players.Length-1){
			
			currentPlayer = players[0];
			nextStage();

		}
		else {
			currentPlayer = players[currentPlayerIndex+1];
		}
		
	}
	
	public void nextStage() {
		Debug.Log("NEXT STAGE");
		Debug.Log("CURRENT STAGE: " + stages[currentStage]);
		if(Object.ReferenceEquals(stages[currentStage].GetType(), typeof(Test))) {
			Debug.Log("test over");
			highestBidder.discardCard(tentativeBet);
			players = new Player[1]; 
			players[0] = highestBidder;
			currentPlayer = highestBidder;
			playerNum = 1;
			highestBidder = null;
			highestBid = -1;
		}
		
		if(currentStage + 1 == stageNum){
			quest = null;
			finishQuest();
			return;
		}
		else {
			currentStage++;
			if(Object.ReferenceEquals(stages[currentStage].GetType(), typeof(Test))) {
				highestBid = stages[currentStage].getMinBid();
			}
		}
	}
	//Getters and setters
	public int getPlayerInt(Player player) {
		int index = -1;
		for(int i = 0; i < players.Length; i++)
		{
			if(players[i] == currentPlayer)
			{
				index = i;
				break;
			}
		}
		return index;
	}
	public void resetQuest() {
		currentStage = 0;
		stageWeapons = null;
		stages = null;
		stageWeapons = new Card[stageNum][];
		stages = new Card[stageNum];
	}
	public void setSponsor(Player player){
		sponsor = player;
	}
	public void setStages(Card[] newStages){
		stages = newStages;
		stageWeapons = new Card[stages.Length][];
		return;
	}
	public void setStageWeapons(Card[] newStageWeapons){
		
		stageWeapons[currentStage] = newStageWeapons;
		return;
	}
	public void setStage(int i) {
		currentStage = i;
	}
	public void setTentativeBet(Card [] bet) {
		tentativeBet = bet;
	}
	public Player getCurrentPlayer() {
		return currentPlayer;
	}
	
	public bool placeBid(Card [] bid, int freeBids) {
		int totalBet;
		if(bid.Length == null) {
			totalBet = freeBids;
		}
		else {
			totalBet = bid.Length + freeBids;
		}
		if(totalBet <= highestBid) {
			return false;
		}
		highestBid = totalBet;
		highestBidder = currentPlayer;
		return true;
	}
	public int getStageNum() {
		return stageNum;
	}
	public int getPlayerNum() {
		return playerNum;
	}
	public Player getSponsor() {
		return sponsor;
	}
	public Card getStage(int i) {
		return stages[i];
	}
	public Card getCurrentStage() {
		return stages[currentStage];
	}
	public int getCurrentStageNum(){
		return currentStage;
	}
	public int getHighestBid(){
		if(highestBid == -1 && Object.ReferenceEquals(stages[currentStage].GetType(), typeof(Test)))
		{
			highestBid = stages[currentStage].getMinBid();
		}
		return highestBid;
	}
	public Card getQuest() {
		
		return quest;
	}
	public Card[] getStageWeapons(int i) {
		return stageWeapons[i];
	}
	private bool isStageSpecial(int i) {
		Card temp = stages[i];
		for(int j = 0; j < quest.getSpecialNum(); j++) {
			if(quest.getSpecialFoe(j).getName().Equals(temp.getName())) { return true; }
		}
		return false;
	}
	public int getStageBP(int i) {
		int baseBP;
		if(isStageSpecial(i)) { 
			baseBP = stages[i].getAltBP();
		}
		else {
			baseBP = stages[i].getBP();
		}
		int extraBP = 0;
		if(stageWeapons[i][0] != null)
		{
			for(int j = 0; j< stageWeapons[i].Length; j++)
			{
				extraBP = extraBP + stageWeapons[i][j].getBP();
			}
		}
		return (baseBP + extraBP);
	}
	
	public bool isInProgress() { return inProgress;}
}
