# $Id$
# $Revision$
# $Date$
#-------------------------------------------------------------------------------
# TimeQuest constraints to cut all false timing paths across asynchronous 
# clock domains. 

set_false_path -from [get_pins -no_case -compatibility_mode *SPIPhy_MOSIctl\|stsourcedata*\|*] -to [get_registers *]
set_false_path -from [get_pins -no_case -compatibility_mode *SPIPhy_altera_avalon_st_idle_inserter\|received_esc*\|*] -to [get_pins -no_case -compatibility_mode *SPIPhy_MISOctl\|rdshiftreg*\|*]
