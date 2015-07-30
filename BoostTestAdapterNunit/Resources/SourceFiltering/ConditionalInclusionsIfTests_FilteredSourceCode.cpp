


//std:cout' that are commented "should not filtered" should not be filtered. The others we assume that have to be filtered

int _tmain(int argc, _TCHAR* argv[])
{

#region testing positive if along with an else statement. The "Hello 02" is expected to be left unfiltered


    std:cout << "Hello 01\n"; //should not be filtered




#endregion


#region testing negative if along with an else statement. The "Hello 04" is expected to be left unfiltered




    std:cout << "Hello 04\n"; //should not be filtered


#endregion

#region testing simple elif. The "Hello 06" is expected to be left unfiltered




    std:cout << "Hello 06\n"; //should not be filtered




#endregion testing a more complex elif. The "Hello 10" is expected to be left unfiltered






    std:cout << "Hello 10\n"; //should not be filtered




#endregion

#region testing a case where if and elif failed, The "Hello 15" is expected to be left unfiltered








    std:cout << "Hello 15\n"; //should not be filtered


#endregion

#region testing of medium complexity nesting with only if, elif and endif


    std:cout << "Hello 16\n"; //should not be filtered
    //-->
























































































        std:cout << "Hello 48\n"; //should not be filtered




            std:cout << "Hello 50\n"; //should not be filtered



            std:cout << "Hello 51\n"; //should not be filtered







            std:cout << "Hello 54\n"; //should not be filtered









            //-->
            std:cout << "Hello 58\n"; //should not be filtered




                std:cout << "Hello 60\n"; //should not be filtered



                std:cout << "Hello 61\n"; //should not be filtered







                std:cout << "Hello 64\n"; //should not be filtered









                std:cout << "Hello 68\n"; //should not be filtered




            std:cout << "Hello 69\n"; //should not be filtered



                std:cout << "Hello 71\n"; //should not be filtered


                std:cout << "Hello 72\n"; //should not be filtered






                std:cout << "Hello 75\n"; //should not be filtered








                std:cout << "Hello 79\n"; //should not be filtered
























































            std:cout << "Hello 102\n"; //should not be filtered



                std:cout << "Hello 104\n"; //should not be filtered


                std:cout << "Hello 105\n"; //should not be filtered






                std:cout << "Hello 108\n"; //should not be filtered








                std:cout << "Hello 112\n"; //should not be filtered

















































































            std:cout << "Hello 146\n"; //should not be filtered



                std:cout << "Hello 148\n"; //should not be filtered


                std:cout << "Hello 149\n"; //should not be filtered






                std:cout << "Hello 152\n"; //should not be filtered








                std:cout << "Hello 156\n"; //should not be filtered


























































        std:cout << "Hello 179\n"; //should not be filtered



            std:cout << "Hello 181\n"; //should not be filtered


            std:cout << "Hello 182\n"; //should not be filtered






            std:cout << "Hello 185\n"; //should not be filtered








            std:cout << "Hello 189\n"; //should not be filtered




#endregion

}