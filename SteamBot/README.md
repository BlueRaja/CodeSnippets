Duplicate Item Detection - 

Add `ItemDuplicates.cs` to your project

A few changes need to be made to the `bot.cs` file:

First, we need to add a variable to the `Bot` class

    public ItemDuplicates DuplicateItems = null;
	
Next, after we download the Schema, we want to pull in known duplicates:

    if (DuplicateItems == null)
    {
        log.Info("Loading Known Duplicate Item Information...");
        DuplicateItems = ItemDuplicates.FetchDuplicates();
        log.Info("Duplicates loaded");
    }
	
Finally, in your user handler, you need to check whether or not this item is a duplicate. This appears in my `Validate` method:

    if (Bot.DuplicateItems.isDuplicate(item.OriginalId))   // No Hacked/Duplicated Items
    {
        errors.Add("Item " + schemaItem.Name + " appears to be duplicated and can not be accepted at this time.");
    }
	
