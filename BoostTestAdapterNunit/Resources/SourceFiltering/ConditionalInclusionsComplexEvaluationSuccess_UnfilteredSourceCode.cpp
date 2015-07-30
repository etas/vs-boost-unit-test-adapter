//all defines are expected to be read and filtered
#define VERSION  
#define LEVEL 19
#define EVER ;;
#define HALF(x) x/2
#define THIRD(x) ((x)/(3))
#define BIG (512)
#define DEBUG
#define SIN(x) sin(x)
#define MAX(x,y) ( (x) > (y) ? (x) : (y) )
#define CUBE(a) ( (a) * (a) * (a) )
#define PRINT cout << #x
#define fPRINT(x) f ## x ## Print

//multiline defines are expected to be filtered
#define ASSERT(x) \
if (! (x)) \
{ \
     cout << "ERROR!! Assert " << #x << " failed << endl; \
     cout << " on line " << __LINE__ << endl; \
     cout << " in file " << __FILE__ << endl; \
}

#pragma region testing whether the tokens are defined

#if defined VERSION
    cout << "VERSION defined";
#endif

#if defined LEVEL
    cout << "LEVEL defined";
#endif

#if defined EVER
    cout << "EVER defined";
#endif

#if defined HALF
    cout << "HALF defined";
#endif

#if defined THIRD
    cout << "THIRD defined";
#endif

#if defined BIG
    cout << "BIG defined";
#endif

#if defined DEBUG
    cout << "DEBUG defined";
#endif

#if defined SIN
    cout << "SIN defined";
#endif

#if defined MAX
    cout << "SIN defined";
#endif

#if defined CUBE
    cout << "CUBE defined";
#endif

#if defined PRINT
    cout << "PRINT defined";
#endif

#if defined fPRINT
    cout << "fPRINT defined";
#endif

#if defined ASSERT
    cout << "ASSERT defined";
#endif

#pragma endregion testing whether the tokens are defined

#pragma region correct evaluation testing

#if LEVEL > 18
    cout << "correct evaluation";
#else
    cout << "incorrect evaluation";
#endif

#if LEVEL > (18)
    cout << "correct evaluation";
#else
    cout << "incorrect evaluation";
#endif

#if LEVEL > LEVEL/2
    cout << "correct evaluation";
#else
    cout << "incorrect evaluation";
#endif

#if LEVEL > (LEVEL/2)
    cout << "correct evaluation";
#else
    cout << "incorrect evaluation";
#endif

#if BIG == 512
    cout << "correct evaluation";
#else
    cout << "incorrect evaluation";
#endif

#pragma endregion correct evaluation testing