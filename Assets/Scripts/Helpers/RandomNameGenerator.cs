﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomNameGenerator {

	private static string[] m_verbs = {
		"Able",
		"Acceptable",
		"Accurate",
		"Actual",
		"Afraid",
		"Aggressive",
		"Alive",
		"Angry",
		"Annoying",
		"Anxious",
		"Asleep",
		"Aware",
		"Basic",
		"Boring",
		"Capable",
		"Careful",
		"Civil",
		"Colourful",
		"Competitive",
		"Confident",
		"Critical",
		"Curious",
		"Cute",
		"Dangerous",
		"Dark",
		"Dramatic",
		"Educated",
		"Efficient",
		"Electrical",
		"Embarrassed",
		"Emotional",
		"Excited",
		"Expensive",
		"Famous",
		"Fancy",
		"Fast",
		"Fierce",
		"Friendly",
		"Guilty",
		"Happy",
		"Healthy",
		"Heavy",
		"Helpful",
		"Huge",
		"Hungry",
		"Impressive",
		"Intelligent",
		"Jolly",
		"Kind",
		"Large",
		"Lawless",
		"Legal",
		"Light",
		"Logical",
		"Lonely",
		"Long",
		"Lucky",
		"Mad",
		"Massive",
		"Medical",
		"Mysterious",
		"Nervous",
		"Nice",
		"Old",
		"Pleasant",
		"Popular",
		"Powerful",
		"Pure",
		"Rare",
		"Realistic",
		"Reasonable",
		"Recent",
		"Relevant",
		"Remarkable",
		"Responsible",
		"Scared",
		"Scrawny",
		"Serious",
		"Slow",
		"Strong",
		"Successful",
		"Suitable",
		"Suspicious",
		"Tall",
		"Terrible",
		"Tiny",
		"Transparent",
		"Typical",
		"Ugly",
		"Unfair",
		"Unhappy",
		"Unusual",
		"Useful",
		"Various",
		"Visible",
		"Weak",
		"Willing",
		"Wonderful",
		"Wooden",
										"Worried"
										};
	private static string[] m_noum = {
		"Aardvark",
		"Alligator",
		"Ash",
		"Ball",
		"Barrel",
		"Basket",
		"Bat",
		"Bee",
		"Belt",
		"Bike",
		"Book",
		"Box",
		"Bread",
		"Brocolli",
		"Bronze",
		"Brush",
		"Cabbage",
		"Cable",
		"Candle",
		"Card",
		"Carrot",
		"Cat",
		"Chicken",
		"Chimney",
		"Clam",
		"Computer",
		"Couch",
		"Cow",
		"Crab",
		"Cup",
		"Deer",
		"Dog",
		"Dog",
		"Dolphin",
		"Door",
		"Duck",
		"Elephant",
		"Fairy",
		"Falcon",
		"Fly",
		"Fork",
		"Fox",
		"Frog",
		"Gecko",
		"Glass",
		"Glove",
		"Glue",
		"Goat",
		"Goffer",
		"Gold",
		"Goose",
		"Gum",
		"Jacket",
		"Jelly",
		"Key",
		"Knife",
		"Leg",
		"Lemon",
		"Lime",
		"Lion",
		"Lizard",
		"Metal",
		"Monitor",
		"Monkey",
		"Moon",
		"Mouse",
		"Nail",
		"Oven",
		"Owl",
		"Paint",
		"Parrot",
		"Penguin",
		"Piano",
		"Pigeon",
		"Plate",
		"Radio",
		"Rhino",
		"Ring",
		"Salmon",
		"Sandal",
		"Seal",
		"Shark",
		"Shovel",
		"Snake",
		"Soap",
		"Sock",
		"Soup",
		"Sponge",
		"Square",
		"Steak",
		"Tiger",
		"Tomato",
		"Toothpaste",
		"Tree",
		"Trolley",
		"Tube",
		"Turkey",
		"Water",
		"Wheat",
		"Zebra"
	};

	public static string GetRanomName()
	{
		string randomVer = m_verbs[Random.Range(0, m_verbs.Length)];
		string randomNoum = m_noum[Random.Range(0, m_noum.Length)];
		return randomVer + randomNoum;
	}
}
