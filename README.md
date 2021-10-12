# recipe-map-playback

 A Potion Craft mod that records a recipe as you are making it, and saves it to a file. The file is a "CSV" file that starts with some metadata about the potion (so not a true CSV, but close). Also adds some text in-game to display the current cost, stress, and health of the potion as you are making it. When you click "Save recipe", it will save the recorded potion recipe to a file, named by the effects if not given a custom title (default potion name in the code). If your potion fails, it will save it as a failed potion (this will get overwritten if a subsequent potion fails). This is a work-in-progress, and the format may change significantly. In particular, it may be reduced in order to save on space, if it's going to be uploaded to a website.
 
 Metadata saved/measured:
 * Price, price without salt, number of ingredients, stress, estimated total path distance, total rotation
 * Potion effects
 * Ingredients used
 * Salts used
 * Tags

 Tags cover the following:
 * Max effect level
 * Number of distinct effects
 * Whether the recipe was successful/failed
 * Whether the recipe was cheated (there is a known bug where full-grind frost sapphires have less than 1 total grind value, so using instant grind on them makes them more ground than is normally possible in game)
 * If the potion is required for any of the alchemy machine recipes (nigredo, albedo, citrinitas, rubedo, philosopher's stone, void salt, life salt)
 * Challenge categories, including:
	- Ingredient category restrictions (mixed, organic, herbal, fungal, crystalline)
	- Ingredient count restrictions (any number, highlander, lowlander)
	- Ladle restrictions (wet, dry)
	- Whirlpool restrictions (hot, cold)
	- Salt restrictions (dull, rich)
	- Grinding restrictions (any grind, full-grind, no-grind, mortarless, extreme, extreme cracked)
 
 The actual data records:
 * typeCode (bitwise movement code, will explain below)
 * name (name of the ingredient + underscore + grinding status, or just name for salts and potion bases)
 * x and y coordinates
 * health (of the potion, depleted by bones)
 
 I opted to not include all the data that is actually recorded in-game, because some of it isn't really interpretable, like the "value" used for displaying in the recipe book, and the "heat" of the coals driving the whirlpools. This makes the files take up considerably less space (though they were still well under 1 MB anyway).
 
 The "typeCode" value is a bitwise code which compresses what was previously a bunch of different strings for different movement/event types.
 The bit flags are as follows:
 * stir (1)
 * ladle (2)
 * heat for effect (4)
 * whirlpool (8)
 * teleport (16)
 * philosopher's salt movement (32)
 * salt addition (64)
 * ingredient addition (128)
 
 (I'm storing it as a plaintext number rather than hex or something, which seems kind of silly, but it's one less conversion to have to make, and the space difference isn't very significant considering adding ingredients is the only 3-character code)
 
 Some of these flags can be combined. So for example, an action of stirring while in a moving whirlpool would have a typeCode of 1 + 8 = 9, while an action of ladling while teleporting would have a typeCode of 2 + 16 = 18. This should save on storage space used compared to writing out strings like "Teleport + Ladle", though when the files are read for display, they will convert back to these strings.
