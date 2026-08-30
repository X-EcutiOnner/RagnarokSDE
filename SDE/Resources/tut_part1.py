"""

								Tutorial - Part 1
								Changing tuple values
								
"""

# Here are some definitions used throughout the guide :
# - A database (default.sde) contains tables.
# - A table (item_db, skill_db, etc) contains rows, called tuples.
# - A table contains multiple columns, most notably the model.
# - The model has multiple fields, such as Id, EquipId, Type, etc.

# All the tables can be accessed via their filename. For example :
# item_db.txt/item_db.yml would be item_db. Other tables would be
# mob_db, skill_db, mob_skill_db, etc.

item_db[501, "Name"] = "Test Item!"
skill_db[7, "Element"] = "Ghost"

# The accessors go as follow :
# table_db[id, "model_field"] = value
# table_d[id] = tuple
# tuple["model_field"] = value
# Example :

item_db[501, "Slots"] = 5
item_db[501, "slots"] = 5

# Calling the table indexer returns the model (Item for item_db)
item = item_db[501]
item.AegisName = "test_1"
item.AegisName = item_db[501, "AegisName"].upper()

# Calling GetTuple will return the tuple instead.
tuple = item_db.GetTuple(501);
tuple["AegisName"] = "test_2"
tuple["AegisName"] = item_db[501, "AegisName"].upper()

# Empty fields behave differently now.
# Using tuple["Defense"] will return the integer 0 if not set.
# However, calling item.Defense will return null or "" if not set.
# The model uses string fields, while calling from the tuple
# will attempt to convert to the most likely field. So for
# accuracy, it is best to call from the model itself.

# This will return "None" (null) since it hasn't been set.
print item.Defense

# This will return 0
if (item.Defense is None):
	print tuple["Defense"]