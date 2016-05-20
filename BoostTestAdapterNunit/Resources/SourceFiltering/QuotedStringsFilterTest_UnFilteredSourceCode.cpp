#include "header"

cout << "Newline character: " << newline << "ending" << endl; 
cout << "Tab character: " << tab << "ending" << endl;
cout << "Backspace character: " << backspace << "ending" << endl;
cout << "Backslash character: " << backslash << "ending" << endl;
cout << "Null character: " << nullChar << "ending" << endl;


const wchar_t* raw_wide = LR"(An unescaped " character)";
const char* good_parens = R"xyz()")xyz";
const wchar_t* newline = LR"(hello
goodbye)";
char str[] = "12" "34";
const wchar_t* raw_wide = LR"(An unescaped " character)" + R"(An unescaped " character)";
const char* const multiline = "Hello \
                                World";