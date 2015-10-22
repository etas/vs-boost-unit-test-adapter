#define DLEVEL
#define NDEBUG

//std:cout' that are commented "should not filtered" should not be filtered. The others we assume that have to be filtered

int _tmain(int argc, _TCHAR* argv[])
{

#region testing positive if along with an else statement. The "Hello 02" is expected to be left unfiltered

#if 1
    std:cout << "Hello 01\n"; //should not be filtered
#else
    std:cout << "Hello 02\n"; 
#endif

#endregion


#region testing negative if along with an else statement. The "Hello 04" is expected to be left unfiltered

#if 0
    std:cout << "Hello 03\n";
#else
    std:cout << "Hello 04\n"; //should not be filtered
#endif

#endregion

#region testing simple elif. The "Hello 06" is expected to be left unfiltered

#if 0
    std:cout << "Hello 05\n";
#elif 1
    std:cout << "Hello 06\n"; //should not be filtered
#else
    std:cout << "Hello 07\n";
#endif

#endregion testing a more complex elif. The "Hello 10" is expected to be left unfiltered

#if 0
    std:cout << "Hello 08\n";
#elif 0
    std:cout << "Hello 09\n";
#elif 1
    std:cout << "Hello 10\n"; //should not be filtered
#else
    std:cout << "Hello 11\n"; 
#endif

#endregion

#region testing a case where if and elif failed, The "Hello 15" is expected to be left unfiltered

#if 0
    std:cout << "Hello 12\n";
#elif 0
    std:cout << "Hello 13\n";
#elif 0
    std:cout << "Hello 14\n";
#else
    std:cout << "Hello 15\n"; //should not be filtered
#endif

#endregion

#region testing of medium complexity nesting with only if, elif and endif

#if 1
    std:cout << "Hello 16\n"; //should not be filtered
    //-->
    #if 0
        //-->
        std:cout << "Hello 17\n";
        #if 0
            //-->
            std:cout << "Hello 18\n";
            #if 0
                std:cout << "Hello 19\n"; 
            #else
                std:cout << "Hello 20\n";
            #endif
                
            #if 1
                std:cout << "Hello 21\n";
            #else
                std:cout << "Hello 22\n";
            #endif
                
            #if 0
                std:cout << "Hello 23\n";
            #elif 1
                std:cout << "Hello 24\n";
            #else
                std:cout << "Hello 25\n";
            #endif
                
            #if 0
                std:cout << "Hello 26\n";
            #elif 0
                std:cout << "Hello 27\n";
            #else
                std:cout << "Hello 28\n";
            #endif
                
        #else

            std:cout << "Hello 29\n";

            #if 0
                std:cout << "Hello 30\n"; 
            #else
                std:cout << "Hello 31\n";
            #endif
                
            #if 1
                std:cout << "Hello 32\n";
            #else
                std:cout << "Hello 33\n";
            #endif
                
            #if 0
                std:cout << "Hello 34\n";
            #elif 1
                std:cout << "Hello 35\n";
            #else
                std:cout << "Hello 36\n";
            #endif
                
            #if 0
                std:cout << "Hello 37\n";
            #elif 0
                std:cout << "Hello 38\n";
            #else
                std:cout << "Hello 39\n";
            #endif
        
        #endif

            #if 1
                std:cout << "Hello 40\n";
            #else
                std:cout << "Hello 41\n";
            #endif
            #if 0
                std:cout << "Hello 42\n";
            #elif 1
                std:cout << "Hello 43\n";
            #else
                std:cout << "Hello 44\n";
            #endif
            #if 0
                std:cout << "Hello 45\n";
            #elif 0
                std:cout << "Hello 46\n";
            #else
                std:cout << "Hello 47\n";
            #endif
    #else
        std:cout << "Hello 48\n"; //should not be filtered

        #if 0
            std:cout << "Hello 49\n";
        #else
            std:cout << "Hello 50\n"; //should not be filtered
        #endif
            
        #if 1
            std:cout << "Hello 51\n"; //should not be filtered
        #else
            std:cout << "Hello 52\n";
        #endif
            
        #if 0
            std:cout << "Hello 53\n"; 
        #elif 1
            std:cout << "Hello 54\n"; //should not be filtered
        #else
            std:cout << "Hello 55\n";
        #endif
            
        #if 0
            std:cout << "Hello 56\n"; 
        #elif 0
            std:cout << "Hello 57\n";
        #else
            //-->
            std:cout << "Hello 58\n"; //should not be filtered

            #if 0
                std:cout << "Hello 59\n";
            #else
                std:cout << "Hello 60\n"; //should not be filtered
            #endif
            
            #if 1
                std:cout << "Hello 61\n"; //should not be filtered
            #else
                std:cout << "Hello 62\n";
            #endif
            
            #if 0
                std:cout << "Hello 63\n"; 
            #elif 1
                std:cout << "Hello 64\n"; //should not be filtered
            #else
                std:cout << "Hello 65\n";
            #endif
            
            #if 0
                std:cout << "Hello 66\n"; 
            #elif 0
                std:cout << "Hello 67\n";
            #else
                std:cout << "Hello 68\n"; //should not be filtered
            #endif
        #endif

        #if 1
            std:cout << "Hello 69\n"; //should not be filtered
            #if 0
                std:cout << "Hello 70\n";
            #else
                std:cout << "Hello 71\n"; //should not be filtered
            #endif
            #if 1
                std:cout << "Hello 72\n"; //should not be filtered
            #else
                std:cout << "Hello 73\n"; 
            #endif
            #if 0
                std:cout << "Hello 74\n"; 
            #elif 1
                std:cout << "Hello 75\n"; //should not be filtered
            #else
                std:cout << "Hello 76\n"; 
            #endif
            #if 0
                std:cout << "Hello 77\n"; 
            #elif 0
                std:cout << "Hello 78\n";
            #else
                std:cout << "Hello 79\n"; //should not be filtered
            #endif
        #else
            std:cout << "Hello 80\n"; 
            #if 0
                std:cout << "Hello 81\n"; 
            #else
                std:cout << "Hello 82\n"; 
            #endif
            #if 1
                std:cout << "Hello 83\n"; 
            #else
                std:cout << "Hello 84\n"; 
            #endif
            #if 0
                std:cout << "Hello 85\n"; 
            #elif 1
                std:cout << "Hello 86\n"; 
            #else
                std:cout << "Hello 87\n"; 
            #endif
            #if 0
                std:cout << "Hello 88\n"; 
            #elif 0
                std:cout << "Hello 89\n"; 
            #else
                std:cout << "Hello 90\n"; 
            #endif
        #endif

        #if 0
            std:cout << "Hello 91\n";
            #if 0
                std:cout << "Hello 92\n"; 
            #else
                std:cout << "Hello 93\n"; 
            #endif
            #if 1
                std:cout << "Hello 94\n"; 
            #else
                std:cout << "Hello 95\n"; 
            #endif
            #if 0
                std:cout << "Hello 96\n"; 
            #elif 1
                std:cout << "Hello 97\n"; 
            #else
                std:cout << "Hello 98\n"; 
            #endif
            #if 0
                std:cout << "Hello 99\n"; 
            #elif 0
                std:cout << "Hello 100\n"; 
            #else
                std:cout << "Hello 101\n"; 
            #endif
        #elif 1
            std:cout << "Hello 102\n"; //should not be filtered
            #if 0
                std:cout << "Hello 103\n"; 
            #else
                std:cout << "Hello 104\n"; //should not be filtered
            #endif
            #if 1
                std:cout << "Hello 105\n"; //should not be filtered
            #else
                std:cout << "Hello 106\n"; 
            #endif
            #if 0
                std:cout << "Hello 107\n"; //should be filtered
            #elif 1
                std:cout << "Hello 108\n"; //should not be filtered
            #else
                std:cout << "Hello 109\n";
            #endif
            #if 0
                std:cout << "Hello 110\n";
            #elif 0
                std:cout << "Hello 111\n";
            #else
                std:cout << "Hello 112\n"; //should not be filtered
            #endif
        #else
            std:cout << "Hello 113\n"; 
            #if 0
                std:cout << "Hello 114\n";
            #else
                std:cout << "Hello 115\n";
            #endif
            #if 1
                std:cout << "Hello 116\n";
            #else
                std:cout << "Hello 117\n"; 
            #endif
            #if 0
                std:cout << "Hello 118\n";
            #elif 1
                std:cout << "Hello 119\n";
            #else
                std:cout << "Hello 120\n";
            #endif
            #if 0
                std:cout << "Hello 121\n";
            #elif 0
                std:cout << "Hello 122\n";
            #else
                std:cout << "Hello 123\n"; 
            #endif
        #endif
        #if 0
            std:cout << "Hello 124\n";
            #if 0
                std:cout << "Hello 125\n";
            #else
                std:cout << "Hello 126\n";
            #endif
            #if 1
                std:cout << "Hello 127\n";
            #else
                std:cout << "Hello 128\n";
            #endif
            #if 0
                std:cout << "Hello 129\n";
            #elif 1
                std:cout << "Hello 130\n";
            #else
                std:cout << "Hello 131\n";
            #endif
            #if 0
                std:cout << "Hello 132\n";
            #elif 0
                std:cout << "Hello 133\n";
            #else
                std:cout << "Hello 134\n";
            #endif
        #elif 0
            std:cout << "Hello 135\n"; 
            #if 0
                std:cout << "Hello 136\n";
            #else
                std:cout << "Hello 137\n";
            #endif
            #if 1
                std:cout << "Hello 138\n";
            #else
                std:cout << "Hello 139\n";
            #endif
            #if 0
                std:cout << "Hello 140\n";
            #elif 1
                std:cout << "Hello 141\n";
            #else
                std:cout << "Hello 142\n";
            #endif
            #if 0
                std:cout << "Hello 143\n";
            #elif 0
                std:cout << "Hello 144\n";
            #else
                std:cout << "Hello 145\n";
            #endif
        #else
            std:cout << "Hello 146\n"; //should not be filtered
            #if 0
                std:cout << "Hello 147\n";
            #else
                std:cout << "Hello 148\n"; //should not be filtered
            #endif
            #if 1
                std:cout << "Hello 149\n"; //should not be filtered
            #else
                std:cout << "Hello 150\n";
            #endif
            #if 0
                std:cout << "Hello 151\n";
            #elif 1
                std:cout << "Hello 152\n"; //should not be filtered
            #else
                std:cout << "Hello 153\n";
            #endif
            #if 0
                std:cout << "Hello 154\n";
            #elif 0
                std:cout << "Hello 155\n";
            #else
                std:cout << "Hello 156\n"; //should not be filtered
            #endif
        #endif
    #endif

    #if 0
    #elif 0
        std:cout << "Hello 157\n"; 
        #if 0
            std:cout << "Hello 158\n"; 
        #else
            std:cout << "Hello 159\n"; 
        #endif
        #if 1
            std:cout << "Hello 160\n";
        #else
            std:cout << "Hello 161\n";
        #endif
        #if 0
            std:cout << "Hello 162\n";
        #elif 1
            std:cout << "Hello 163\n";
        #else
            std:cout << "Hello 164\n";
        #endif
        #if 0
            std:cout << "Hello 165\n";
        #elif 0
            std:cout << "Hello 166\n";
        #else
            std:cout << "Hello 167\n"; 
        #endif
    #elif 0
        std:cout << "Hello 168\n";
        #if 0
            std:cout << "Hello 169\n";
        #else
            std:cout << "Hello 170\n";
        #endif
        #if 1
            std:cout << "Hello 171\n";
        #else
            std:cout << "Hello 172\n";
        #endif
        #if 0
            std:cout << "Hello 173\n";
        #elif 1
            std:cout << "Hello 174\n";
        #else
            std:cout << "Hello 175\n";
        #endif
        #if 0
            std:cout << "Hello 176\n";
        #elif 0
            std:cout << "Hello 177\n";
        #else
            std:cout << "Hello 178\n";
        #endif
    #else
        std:cout << "Hello 179\n"; //should not be filtered
        #if 0
            std:cout << "Hello 180\n";
        #else
            std:cout << "Hello 181\n"; //should not be filtered
        #endif
        #if 1
            std:cout << "Hello 182\n"; //should not be filtered
        #else
            std:cout << "Hello 183\n";
        #endif
        #if 0
            std:cout << "Hello 184\n";
        #elif 1
            std:cout << "Hello 185\n"; //should not be filtered
        #else
            std:cout << "Hello 186\n";
        #endif
        #if 0
            std:cout << "Hello 187\n"; 
        #elif 0
            std:cout << "Hello 188\n";
        #else
            std:cout << "Hello 189\n"; //should not be filtered
        #endif
    #endif
#endif

#endregion

}