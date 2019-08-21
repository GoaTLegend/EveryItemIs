using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Gungeon;

namespace EveryItemIs
{
    public class MainModule : ETGModule
    {
		private static string modVersion = "1.0.1";
		private static int glassGuonID;
		private static int halfHeartID;
		private static int heartID;
		private static int shieldID;
		private static int blankID;
		private static int ammoID;
		private static int spreadAmmoID;
		private static int keyID;
		private static int mapItemID;
		private static int junkID;
		private static int crestID;
		private static int[] masterRoundIDs;
		private static List<PickupObject> itemPool = new List<PickupObject>();
		private static List<int> safeItems = new List<int>();
		private static bool challengeEnabled = true;
		private static bool[] settingBools =
		{
			false,
			false,
			false,
			true,
			true,
			true,
			false,
			false,
			false,
			false,
			false,
			true,
			true,
			false
		};
		private static string[] availableSettings =
		{
			"REPLACEMASTERROUND",
			"REPLACEJUNK",
			"REPLACEGLASSGUON",
			"REPLACEGUNS",
			"REPLACEPASSIVES",
			"REPLACEACTIVES",
			"SHOPREPLACEHEALTH",
			"SHOPREPLACESHIELDS",
			"SHOPREPLACEBLANKS",
			"SHOPREPLACEAMMO",
			"SHOPREPLACEKEYS",
			"SHOPADJUSTPRICE",
			"REPLACEMAPS",
			"REPLACECREST"
		};
		private static string[] availableSettingsShort =
		{
			"RMR",
			"RJ",
			"RGG",
			"RG",
			"RP",
			"RA",
			"SRH",
			"SRS",
			"SRB",
			"SRA",
			"SRK",
			"SAP",
			"RM",
			"RC"
		};

		protected static AutocompletionSettings ItemAutocompletionSettings = new AutocompletionSettings((Func<string, string[]>)(input =>
        {
            List<string> stringList = new List<string>();
            foreach (string id in Game.Items.IDs)
            {
                if (id.AutocompletionMatch(input.ToLower()))
                {
                    Console.WriteLine(string.Format("INPUT {0} KEY {1} MATCH!", (object)input, (object)id));
                    stringList.Add(id.Replace("gungeon:", ""));
                }
                else
                    Console.WriteLine(string.Format("INPUT {0} KEY {1} NO MATCH!", (object)input, (object)id));
            }
            return stringList.ToArray();
        }));

		protected static AutocompletionSettings RemoveAutocompletionSettings = new AutocompletionSettings((Func<string, string[]>)(input =>
		{
			List<string> stringList = new List<string>();
			foreach (PickupObject idItem in itemPool)
			{
				string id = idItem.name.ToLower();
				if (id.AutocompletionMatch(input.ToLower()))
				{
					Console.WriteLine(string.Format("INPUT {0} KEY {1} MATCH!", (object)input, (object)id));
					stringList.Add(id.Replace("gungeon:", ""));
				}
				else
					Console.WriteLine(string.Format("INPUT {0} KEY {1} NO MATCH!", (object)input, (object)id));
			}

			return stringList.ToArray();
		}));

		protected static AutocompletionSettings SettingsAutocompletionSettings = new AutocompletionSettings((Func<string, string[]>)(input =>
		{
			List<string> stringList = new List<string>();
			foreach (string id in availableSettings)
			{
				if (id.ToLower().AutocompletionMatch(input.ToLower()))
				{
					Console.WriteLine(string.Format("INPUT {0} KEY {1} MATCH!", (object)input, (object)id));
					stringList.Add(id);
				}
				else
					Console.WriteLine(string.Format("INPUT {0} KEY {1} NO MATCH!", (object)input, (object)id));
			}
			return stringList.ToArray();
		}));

		public override void Init()
        {
            
        }

        public override void Start()
        {
			ETGModConsole.Commands.AddGroup("itempool");
			ETGModConsole.Commands.GetGroup("itempool").AddUnit("add", AddItemToPool, ItemAutocompletionSettings);
			ETGModConsole.Commands.GetGroup("itempool").AddGroup("remove", RemoveItemFromPool, RemoveAutocompletionSettings);
			ETGModConsole.Commands.GetGroup("itempool").GetGroup("remove").AddUnit("all", EmptyPool);
			ETGModConsole.Commands.GetGroup("itempool").AddUnit("list", ListPool);
			ETGModConsole.Commands.AddGroup("challenge");
			ETGModConsole.Commands.GetGroup("challenge").AddUnit("enable", EnableChallenge);
			ETGModConsole.Commands.GetGroup("challenge").AddUnit("disable", DisableChallenge);
			ETGModConsole.Commands.GetGroup("challenge").AddGroup("settings", ConfigureSettings, SettingsAutocompletionSettings);
			ETGModConsole.Commands.GetGroup("challenge").GetGroup("settings").AddUnit("list", ListSettings);
			ETGModConsole.Commands.GetGroup("challenge").GetGroup("settings").AddUnit("quick", QuickSettings);

			glassGuonID = Game.Items.Get("glass_guon_stone").PickupObjectId;
			halfHeartID = Game.Items.Get("half_heart").PickupObjectId;
			heartID = Game.Items.Get("heart").PickupObjectId;
			shieldID = Game.Items.Get("armor").PickupObjectId;
			blankID = Game.Items.Get("blank").PickupObjectId;
			ammoID = Game.Items.Get("ammo").PickupObjectId;
			spreadAmmoID = Game.Items.Get("partial_ammo").PickupObjectId;
			keyID = Game.Items.Get("key").PickupObjectId;
			mapItemID = Game.Items.Get("map").PickupObjectId;
			junkID = Game.Items.Get("junk").PickupObjectId;
			crestID = Game.Items.Get("old_crest").PickupObjectId;
			masterRoundIDs = new int[] {
				Game.Items.Get("master_round_1").PickupObjectId,
				Game.Items.Get("master_round_2").PickupObjectId,
				Game.Items.Get("master_round_3").PickupObjectId,
				Game.Items.Get("master_round_4").PickupObjectId,
				Game.Items.Get("master_round_5").PickupObjectId
			};

			if (settingBools.Length != availableSettings.Length || availableSettingsShort.Length != availableSettings.Length)
			{
				ETGModConsole.Log("that goat's an idiot");
			}

			ETGModConsole.Log("EveryItemIsMod (version: " + modVersion + ") has been initialiazed!");
        }

        public override void Exit()
        {
            
        }

		public static void QuickSettings (string[] args)
		{
			if (args.Length == 0)
			{
				ETGModConsole.Log("Usage: Argument must be the short id of a setting followed by 'T' or 'F' to update settings accordingly \n" +
					"Example: 'challenge settings quick RJT RGGF' would set REPLACEJUNK to 'True' and REPLACEGLASSGUON to 'False'");
				return;
			}

			for (int i = 0; i < args.Length; i++)
			{
				string argument = args[i].ToLower().Replace("'", "");
				for (int j = 0; j < availableSettingsShort.Length; j++)
				{
					if (availableSettingsShort[j].ToLower() == (argument.Substring(0, args[i].Length - 1)))
					{
						if (argument.EndsWith("t")) {
							settingBools[j] = true;
							ETGModConsole.Log("  " + availableSettings[j] + " (" + availableSettingsShort[j] + ") : " + settingBools[j]);
						} else if (argument.EndsWith("f"))
						{
							settingBools[j] = false;
							ETGModConsole.Log("  " + availableSettings[j] + " (" + availableSettingsShort[j] + ") : " + settingBools[j]);
						} else
						{
							ETGModConsole.Log("Short id of argument must be directly followed by 'T' or 'F' \n" +
								"Example: 'challenge settings quick " + availableSettingsShort[j] + "T' would set " + availableSettings[j] + " to 'True'");
						}
						break;
					} else if (j == availableSettingsShort.Length-1)
					{
						ETGModConsole.Log("No setting corresponds to " + args[i]);
					}
				}
			}
		}

		public static void ConfigureSettings(string[] args)
		{
			if (args.Length == 0)
			{
				ETGModConsole.Log("Argument after 'settings' must be a setting name. Example: 'challenge settings REPLACEJUNK true'");
				return;
			}

			if (!GetSetting(args[0]).HasValue)
			{
				ETGModConsole.Log("That setting doesn't exist");
				return;
			}

			if (args.Length == 2)
			{
				if (args[1].ToLower() == "true" || args[1].ToLower() == "t")
				{
					settingBools[(int)GetSetting(args[0])] = true;
				} else if (args[1].ToLower() == "false" || args[1].ToLower() == "f")
				{
					settingBools[(int)GetSetting(args[0])] = false;
				} else
				{
					ETGModConsole.Log("Argument after setting name must be either 'true' or 'false'. Example: 'challenge settings " + availableSettings[(int)GetSetting(args[0])] + " true'");
					return;
				}
			}
			if (args.Length >= 1)
			{
				ETGModConsole.Log("  " + availableSettings[(int)GetSetting(args[0])] + " (" + availableSettingsShort[(int)GetSetting(args[0])] + ") : " + settingBools[(int)GetSetting(args[0])]);
			} else
			{
				ETGModConsole.Log("You need to specify a subcommand or a rule to configure");
			}
		}

		public static int? GetSetting(string setting)
		{
			for (int i = 0; i < availableSettings.Length; i++)
			{
				if (availableSettings[i].ToLower() == setting.ToLower())
				{
					return i;
				}
			}
			return null;
		}

		public static void ListSettings(string[] args)
		{
			string quickString = "";
			for (int i = 0; i < availableSettings.Length; i++)
			{
				ETGModConsole.Log("  " + availableSettings[i] + " (" + availableSettingsShort[i] + ") : " + settingBools[i]);
				quickString = quickString + availableSettingsShort[i] + (settingBools[i] ? "T" : "F") + " ";
			}
			ETGModConsole.Log(" Current quick setting is: '" + quickString + "'");
		}

		public static void DisableChallenge(string[] args)
		{
			if (MakeSurent() == true)
			{
				ETGModConsole.Log("Challenge is now OFF");
				challengeEnabled = false;
			}
		}

		public static void EnableChallenge(string[] args)
		{
			if (MakeSure() == true)
			{
				ETGModConsole.Log("Challenge is now ON");
				challengeEnabled = true;
			}
		}

		public static bool MakeSure()
		{
			if (GameManager.Instance != null)
			{
				if (GameManager.Instance.PrimaryPlayer != null)
				{
					if (!GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<ChallengePerformer>())
					{
						GameManager.Instance.PrimaryPlayer.gameObject.AddComponent<ChallengePerformer>().StartCoroutine(ChallengePerformer.PerformStuff());
						return true;
					} else
					{
						if (!GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<ChallengePerformer>().isActiveAndEnabled)
						{
							GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<ChallengePerformer>().enabled = true;
							GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<ChallengePerformer>().StartCoroutine(ChallengePerformer.PerformStuff());
						}
						return true;
					}
				} else
				{
					ETGModConsole.Log("Primary player is null, only use inside a run and not in the breach");
					return false;
				}
			} else
			{
				ETGModConsole.Log("GameManager is null, only use inside a run and not in the breach");
				return false;
			}
		}

		public static bool MakeSurent()
		{
			if (GameManager.Instance != null)
			{
				if (GameManager.Instance.PrimaryPlayer != null)
				{
					if (!GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<ChallengePerformer>())
					{
						GameManager.Instance.PrimaryPlayer.gameObject.AddComponent<ChallengePerformer>().enabled = false;
						return true;
					}
					else
					{
						if (GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<ChallengePerformer>().isActiveAndEnabled)
						{
							GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<ChallengePerformer>().StopAllCoroutines();
							GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<ChallengePerformer>().enabled = false;
						}
						return true;
					}
				}
				else
				{
					ETGModConsole.Log("Primary player is null, only use inside a run and not in the breach");
					return false;
				}
			}
			else
			{
				ETGModConsole.Log("GameManager is null, only use inside a run and not in the breach");
				return false;
			}
		}

		public static void EmptyPool(string[] args)
		{
			itemPool = new List<PickupObject>();
		}

		public static void ListPool(string[] args)
		{
			foreach (PickupObject item in itemPool)
			{
				ETGModConsole.Log("  " + item.DisplayName + " (" + item.name + ")");
			}
		}

		public void UpdateSafeItems (int instanceID)
		{
			if (!safeItems.Contains(instanceID))
			{
				safeItems.Add(instanceID);
			}
		}

		public static void AddItemToPool(string[] args)
		{
			if (args.Length == 0)
			{
				ETGModConsole.Log("Usage: puts an item into the pool of items to be picked from when replacing an item. \n" +
					"Example: 'itempool add gungeon_pepper' would add the gungeon pepper to the itempool");
				return;
			}

			string id = args[0];

			if (!Game.Items.ContainsID(id))
			{
				ETGModConsole.Log(string.Format("Invalid item ID {0}", id));
			} else
			{
				itemPool.Add(Game.Items.Get(id));
				ETGModConsole.Log(string.Format("{0} has been added to the pool!", id));
			}
			if (challengeEnabled == true)
			{
				MakeSure();
			}
		}

		public static void RemoveItemFromPool(string[] args)
		{
			if (args.Length == 0)
			{
				ETGModConsole.Log("Usage: removes an item from the pool of items to be picked from when replacing an item. \n" +
					"Example: 'itempool remove gungeon_pepper' would remove the gungeon pepper from the itempool");
				return;
			}

			string name = args[0];
			foreach (PickupObject item in itemPool)
			{
				if (item.name == name)
				{
					itemPool.Remove(item);
					ETGModConsole.Log(item.DisplayName + " has been removed from the itempool");
					return;
				}
			}
			if (challengeEnabled == true)
			{
				MakeSure();
			}
			ETGModConsole.Log("The itempool does not contain " + name);
		}

		public static void AllItemsAre()
        {
			if (itemPool.Count == 0)
			{
				return;
			}

            foreach (PickupObject pickupObject in UnityEngine.Object.FindObjectsOfType<PickupObject>())
            {
                //ETGModConsole.Log("ID: " + pickupObject.PickupObjectId + " | InstanceID: " + pickupObject.GetInstanceID() + " | Name: " + pickupObject.name);

				bool continueThen = false;
				foreach (PickupObject item in itemPool)
				{
					if (item.PickupObjectId == pickupObject.PickupObjectId)
					{
						continueThen = true;
						break;
					}
				}

				if (GameManager.Instance.PrimaryPlayer != null)
				{
					if (GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Pilot)
					{
						if (pickupObject.PickupObjectId == Game.Items.Get("trusty_lockpicks").PickupObjectId)
						{
							continueThen = true;
						}
					} else if (GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Convict)
					{
						if (pickupObject.PickupObjectId == Game.Items.Get("molotov").PickupObjectId)
						{
							continueThen = true;
						}
					} else if (GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Soldier)
					{
						if (pickupObject.PickupObjectId == Game.Items.Get("supply_drop").PickupObjectId)
						{
							continueThen = true;
						}
					} else if (GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Robot)
					{
						if (pickupObject.PickupObjectId == Game.Items.Get("coolant_leak").PickupObjectId)
						{
							continueThen = true;
						}
					}
				}

				if (!settingBools[2] && pickupObject.PickupObjectId == glassGuonID)
				{
					continue;
				} else if (!settingBools[12] && pickupObject.PickupObjectId == mapItemID)
				{
					continue;
				} else if (!settingBools[1] && pickupObject.PickupObjectId == junkID)
				{
					continue;
				} else if (pickupObject.PickupObjectId == keyID)
				{
					continue;
				} else if (pickupObject.PickupObjectId == heartID)
				{
					continue;
				} else if (pickupObject.PickupObjectId == GlobalItemIds.Blank)
				{
					continue;
				} else if (pickupObject.PickupObjectId == halfHeartID)
				{
					continue;
				}

				if (continueThen)
				{
					continue;
				}


                if (pickupObject as Gun != null && settingBools[3])
                {
                    Gun gun = pickupObject.GetComponent<Gun>();
					if (gun.HasBeenPickedUp)
					{
						safeItems.Add(gun.GetInstanceID());
						continue;
					} else
					{
						if (!safeItems.Contains(gun.GetInstanceID()))
						{
							LootEngine.SpawnItem(itemPool.ElementAt(UnityEngine.Random.Range(0, itemPool.Count)).gameObject, pickupObject.transform.position, Vector2.up, 1.0f);
							UnityEngine.Object.Destroy(gun.gameObject);
						}
					}
                }
                else if (pickupObject as PassiveItem != null && settingBools[4])
				{
					PassiveItem passiveItem = pickupObject.GetComponent<PassiveItem>();
					if (passiveItem.PickedUp)
					{
						safeItems.Add(passiveItem.GetInstanceID());
						continue;
					}
					else
					{
						if (!safeItems.Contains(passiveItem.GetInstanceID()))
						{
							{
								LootEngine.SpawnItem(itemPool.ElementAt(UnityEngine.Random.Range(0, itemPool.Count)).gameObject, pickupObject.transform.position, Vector2.up, 1.0f);
								UnityEngine.Object.Destroy(passiveItem.gameObject);
							}
						}
					}
				}
				else if (pickupObject as PlayerItem != null && settingBools[5])
				{
					PlayerItem playerItem = pickupObject.GetComponent<PlayerItem>();
					if (playerItem.LastOwner != null)
					{
						safeItems.Add(playerItem.GetInstanceID());
						continue;
					}
					else
					{
						if (!safeItems.Contains(playerItem.GetInstanceID()))
						{
							ETGModConsole.Log("egg");
							LootEngine.SpawnItem(itemPool.ElementAt(UnityEngine.Random.Range(0, itemPool.Count)).gameObject, pickupObject.transform.position, Vector2.up, 1.0f);
							UnityEngine.Object.Destroy(playerItem.gameObject);
						}
					}
				}
				else
                {
                    continue;
                }
            }
			
			foreach (ShopItemController shopItem in UnityEngine.Object.FindObjectsOfType<ShopItemController>())
			{
				bool continueSoon = false;
				foreach (PickupObject itemToTest in itemPool)
				{
					if (itemToTest.PickupObjectId == shopItem.item.PickupObjectId)
					{
						continueSoon = true;
					}
				}

				if (!settingBools[3] && shopItem.item is Gun)
				{
					continue;
				} else if (!settingBools[4] && shopItem.item is PassiveItem)
				{
					continue;
				} else if (!settingBools[5] && shopItem.item is PlayerItem)
				{
					continue;
				}

				if (continueSoon)
				{
					continue;
				} else
				{
					if (StaticReferenceManager.AllShops.Count > 0)
					{
						if (!settingBools[6] && (shopItem.item.PickupObjectId == halfHeartID || shopItem.item.PickupObjectId == heartID))
						{
							continue;
						} else if (!settingBools[7] && shopItem.item.PickupObjectId == shieldID)
						{
							continue;
						} else if (!settingBools[8] && shopItem.item.PickupObjectId == blankID)
						{
							continue;
						} else if (!settingBools[9] && (shopItem.item.PickupObjectId == ammoID || shopItem.item.PickupObjectId == spreadAmmoID))
						{
							continue;
						} else if (!settingBools[10] && shopItem.item.PickupObjectId == keyID)
						{
							continue;
						} else if (!settingBools[2] && shopItem.item.PickupObjectId == glassGuonID)
						{
							continue;
						}
						else if (!settingBools[12] && shopItem.item.PickupObjectId == mapItemID)
						{
							continue;
						}
						else if (!settingBools[1] && shopItem.item.PickupObjectId == junkID)
						{
							continue;
						}
						else if (shopItem.IsResourcefulRatKey)
						{
							continue;
						} else
						{
							float distanceToClosest = 555555f;
							BaseShopController shopToUse = StaticReferenceManager.AllShops.First();
							foreach (BaseShopController shop in StaticReferenceManager.AllShops)
							{
								foreach (Transform tran in shop.spawnPositions)
								{
									if (shopItem.GetDistanceToPoint(tran.position) < distanceToClosest)
									{
										distanceToClosest = shopItem.GetDistanceToPoint(tran.position);
										shopToUse = shop;
									}
								}
								foreach (Transform tran in shop.spawnPositionsGroup2)
								{
									if (shopItem.GetDistanceToPoint(tran.position) < distanceToClosest)
									{
										distanceToClosest = shopItem.GetDistanceToPoint(tran.position);
										shopToUse = shop;
									}
								}
							}
							int priceBefore = shopItem.CurrentPrice;
							shopItem.Initialize(itemPool.ElementAt(UnityEngine.Random.Range(0, itemPool.Count)), shopToUse);
							if (!settingBools[11])
							{
								shopItem.OverridePrice = priceBefore;
							} else if (shopItem.OverridePrice == 9999)
							{
								ETGModConsole.Log("New Payday item in shop... adjusting price to avoid 9999 cost item");
								shopItem.OverridePrice = 69; //heheheheheheh
							}
						}
					}
				}
			}

			foreach (RewardPedestal pedestal in UnityEngine.Object.FindObjectsOfType<RewardPedestal>())
			{
				if (pedestal.pickedUp || pedestal.contents == null)
				{
					continue;
				}
				
				bool conAfter = false;
				foreach (PickupObject idem in itemPool)
				{
					if (pedestal.contents.PickupObjectId == idem.PickupObjectId)
					{
						conAfter = true;
					}
				}
				
				if (conAfter)
				{
					continue;
				}

				if (!settingBools[3] && pedestal.contents is Gun)
				{
					continue;
				}
				else if (!settingBools[4] && pedestal.contents is PassiveItem)
				{
					continue;
				}
				else if (!settingBools[5] && pedestal.contents is PlayerItem)
				{
					continue;
				}
				else if (!settingBools[2] && pedestal.contents.PickupObjectId == glassGuonID)
				{
					continue;
				}
				else if (!settingBools[12] && pedestal.contents.PickupObjectId == mapItemID)
				{
					continue;
				}
				else if (!settingBools[1] && pedestal.contents.PickupObjectId == junkID)
				{
					continue;
				}
				else if (!settingBools[13] && pedestal.contents.PickupObjectId == crestID)
				{
					continue;
				}
				else if (!settingBools[0] && masterRoundIDs.Contains(pedestal.contents.PickupObjectId))
				{
					continue;
				}

				PickupObject idemInstead = itemPool[UnityEngine.Random.Range(0, itemPool.Count)];

				pedestal.contents = null;
				pedestal.IsBossRewardPedestal = false;
				pedestal.UsesSpecificItem = true;
				pedestal.SpecificItemId = idemInstead.PickupObjectId;
				pedestal.ForceConfiguration();

				pedestal.spawnTransform.GetComponentInChildren<tk2dSprite>().sprite.Collection = idemInstead.sprite.Collection;
				pedestal.spawnTransform.GetComponentInChildren<tk2dSprite>().sprite.spriteId = idemInstead.sprite.spriteId;
				SpriteOutlineManager.AddOutlineToSprite(pedestal.spawnTransform.GetComponentInChildren<tk2dSprite>(), Color.black, 0.1f, 0.05f, SpriteOutlineManager.OutlineType.NORMAL);
				pedestal.spawnTransform.GetComponentInChildren<tk2dSprite>().HeightOffGround = 0.25f;
				pedestal.spawnTransform.GetComponentInChildren<tk2dSprite>().depthUsesTrimmedBounds = true;
				pedestal.spawnTransform.GetComponentInChildren<tk2dSprite>().PlaceAtPositionByAnchor(pedestal.spawnTransform.position, tk2dSprite.Anchor.LowerCenter);
				pedestal.spawnTransform.GetComponentInChildren<tk2dSprite>().transform.position = pedestal.spawnTransform.GetComponentInChildren<tk2dSprite>().transform.position.Quantize(1f / 16f);
				pedestal.spawnTransform.GetComponentInChildren<tk2dSprite>().UpdateZDepth();
			}
        }
    }
}
