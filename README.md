# recipe-map-playback

 A Potion Craft mod that records a recipe as you are making it, and saves it to a file. The file is a "CSV" file that starts with some metadata about the potion (so not a true CSV, but close). Also adds some text in-game to display the current cost, stress, and health of the potion as you are making it. When you click "Save recipe", it will save the recorded potion recipe to a file, named by the effects if not given a custom title (default potion name in the code). If your potion fails, it will save it as a failed potion (this will get overwritten if a subsequent potion fails). This is a work-in-progress, and the format may change significantly. In particular, it may be reduced in order to save on space, if it's going to be uploaded to a website.
 
 Metadata saved/measured:
 - Price, price without salt, number of ingredients, stress, estimated total path distance
 - Potion effects
 - Ingredients used
 - Salts used
 - Tags

 Tags cover the following:
 - Max effect level
 - Number of distinct effects
 - Whether the recipe was successful/failed
 - Whether the recipe was cheated (there is a known bug where full-grind frost sapphires have less than 1 total grind value, so using instant grind on them makes them more ground than is normally possible in game)
 - If the potion is required for any of the alchemy machine recipes (nigredo, albedo, citrinitas, rubedo, philosopher's stone, void salt, life salt)
 - Challenge categories, including:
 -- Ingredient category restrictions (mixed, organic, herbal, fungal, crystalline)
 -- Ingredient count restrictions (any number, highlander, lowlander)
 -- Ladle restrictions (wet, dry)
 -- Whirlpool restrictions (hot, cold)
 -- Salt restrictions (dull, rich)
 -- Grinding restrictions (any grind, full-grind, no-grind, mortarless, extreme, extreme cracked)
 
 The actual data records:
 - step (index of the recording)
 - name (either the name of the ingredient, or the name of the motion type, including combinations like teleport + ladle)
 - x and y coordinates
 - typeCode (encoded form of the ingredient/movement type, maybe not necessary)
 - grindState (Full, None, Cracked, Cheated, Other, or Move/Salt for steps that aren't from adding ingredients)
 - value (recorded in the game as a value to add to the notes, but may not be necessary)
 - health (of the potion, depleted by bones)
 - rotation (from the sun and moon salts, obviously not very important yet)
 - teleport (0 or 1, if the potion is teleporting)
 - whirlpool (records the heat if in a whirlpool, otherwise 0)
 - philSalt (0 or 1, if philosopher's salt is being used to move the potion, obviously not very important yet)
 - distance (estimated total distance of the path)
 
 Some of these are not necessary, and some of them could be combined. The metadata starts/ends with a Metadata_start/Metadata_end. This is for readability but not really necessary. 
 
