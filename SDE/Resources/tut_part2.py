"""

								Tutorial - Part 2
								Selection
								
"""

# You can manipulate the selection of the editor and read its values.
# The example below prints the first three elements of the selected items.

if (selection.Count == 0):
	script.throw("Please select an item in the tab!")

# tuple[2] is usually the display binding value
for tuple in selection:
	print script.format("#{0}, {1}, {2}", tuple["Id"], tuple["model"], tuple[2])

# The above will bring up the console output and show you the result.
# selection contains the items selected.
# script.format() is a useful method to format your output result.

# You can use the currently selected table with the following :

if (selected_db != item_db):
	script.throw("Please select the Item tab!")
	
# To read the values of the table, simply iterate through it like a list.

for tuple in selected_db:
	print "Id = " + str(tuple.Key)
	
# This is the same as 

for tuple in item_db:
	print "Id = " + str(tuple[0])
	
# Let's say you want to select items from 500 to 560 :

selection.Clear()
for x in range(500, 561): # 561 is not included
	if (selected_db.ContainsKey(x)):
		selection.Add(selected_db.GetTuple(x))
		
# Or if you want to select all the potion items...쨍

selection.Clear()
for tuple in item_db: # 561 is not included
	#print "Id = " + str(tuple.Key)
	if ("Potion" in tuple["Name"]):
		selection.Add(tuple)
		