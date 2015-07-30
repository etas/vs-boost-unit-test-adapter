//std:cout' that are commented "should not be filtered" should not be filtered. The others we assume that have to be filtered

#ifdef DEBUG
    std:cout << "Hello 01\n"; //should not be filtered
    #undef DEBUG
    std:cout << "Hello 02\n"; //should not be filtered
#endif

#ifndef DEBUG
    std:cout << "Hello 03\n";  //should not be filtered
#elif 0
    std:cout << "Hello 04\n";
#endif

#define DEBUG
#undef DEBUG
#define VER

#ifdef DEBUG
    #define CLIENT1  //tests that defines during a discarded section are not added to the define list
    std:cout << "Hello 05\n";
    #ifdef VER
        std:cout << "Hello 06\n"; 
        #undef VER
        std:cout << "Hello 07\n";
    #endif

    #ifndef VER
        std:cout << "Hello 08\n";
    #elif 0
        std:cout << "Hello 09\n";
    #endif

    #define VER
    #undef VER

    #ifdef VER
        std:cout << "Hello 10\n";
    #elif 1
        std:cout << "Hello 12\n";
    #elif 1
        std:cout << "Hello 13\n";
    #endif
#elif 1
    std:cout << "Hello 14\n"; //should not be filtered
    #ifdef VER
        std:cout << "Hello 15\n"; //should not be filtered
        #undef VER
        std:cout << "Hello 16\n"; //should not be filtered
    #endif

    #ifndef VER
        std:cout << "Hello 17\n";  //should not be filtered
    #elif 0
        std:cout << "Hello 18\n";
    #endif

    #define VER
    #undef VER

    #ifdef VER
        std:cout << "Hello 19\n";
    #elif 0
        std:cout << "Hello 20\n";
    #elif 1
        #define VER
        std:cout << "Hello 21\n"; //should not be filtered
        #if 0
            std:cout << "Hello 22\n"; 
        #else
            std:cout << "Hello 23\n"; //should not be filtered
        #endif
        
        #if 1
            std:cout << "Hello 24\n"; //should not be filtered
        #else
            std:cout << "Hello 25\n";
        #endif
        
        #if 0
            std:cout << "Hello 26\n";
        #elif 0
            std:cout << "Hello 27\n";
        #elif 0
            std:cout << "Hello 28\n";
        #elif 0
            std:cout << "Hello 29\n";
        #elif 0
            std:cout << "Hello 30\n";
        #elif 1
            std:cout << "Hello 31\n"; //should not be filtered
        #else
            std:cout << "Hello 32\n";
        #endif
        
        #if 0
            std:cout << "Hello 33\n";
        #elif 0
            std:cout << "Hello 34\n";
        #else
            std:cout << "Hello 35\n"; //should not be filtered
            #ifdef VER
                std:cout << "Hello 36\n";  //should not be filtered
                #undef VER
                std:cout << "Hello 37\n";  //should not be filtered
            #endif

            #ifndef VER
                std:cout << "Hello 38\n";   //should not be filtered
            #elif 0
                std:cout << "Hello 39\n";
            #endif

            #define VER
            #undef VER

            #ifdef VER
                std:cout << "Hello 40\n";
            #elif 1
                std:cout << "Hello 41\n";   //should not be filtered
            #elif 1
                std:cout << "Hello 42\n";
            #endif

            #define VER

            #if defined VER
                std:cout << "Hello 42\n";  //should not be filtered
                #undef VER
                std:cout << "Hello 43\n";  //should not be filtered
            #endif

            #if !defined VER
                std:cout << "Hello 44\n";   //should not be filtered
            #elif 0
                std:cout << "Hello 45\n";
            #endif

            #define VER
            #undef VER

            #ifdef VER
                std:cout << "Hello 46\n";
            #elif 1
                std:cout << "Hello 47\n";   //should not be filtered
            #elif 1
                std:cout << "Hello 48\n";
            #endif

        #endif
    #elif 1
        std:cout << "Hello 49\n";
    #endif
#elif 1
    std:cout << "Hello 50\n";
#endif