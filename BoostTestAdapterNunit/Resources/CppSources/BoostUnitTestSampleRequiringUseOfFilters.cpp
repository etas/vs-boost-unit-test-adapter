//
// Distributed under the Boost Software License, Version 1.0.
//    (See copy at http://www.boost.org/LICENSE_1_0.txt)
//

#include "stdafx.h"
#include <boost/test/test_case_template.hpp>
#include <boost/mpl/list.hpp>

int add(int i, int j)
{
    return i + j;
}

BOOST_AUTO_TEST_SUITE(Suite1)
    BOOST_AUTO_TEST_CASE(BoostUnitTest123)
{
    BOOST_WARN(sizeof(int) == sizeof(short));
}
BOOST_AUTO_TEST_CASE(BoostUnitTest1234)
{
    BOOST_WARN(sizeof(int) == sizeof(short));
}
BOOST_AUTO_TEST_SUITE_END()

    BOOST_AUTO_TEST_CASE(BoostUnitTest12345)
{
    BOOST_WARN(sizeof(int) == sizeof(short));
}

std::cout << "BOOST_AUTO_TEST_CASE(BoostUnitTestShouldNotAppear1) " << std::endl;

//BOOST_AUTO_TEST_CASE(BoostUnitTestShouldNotAppear2)
//{
//    BOOST_WARN(sizeof(int) == sizeof(short));
//}

typedef boost::mpl::list<int, long, char> test_types;

BOOST_AUTO_TEST_CASE_TEMPLATE(my_test, T, test_types)
{
    BOOST_CHECK_EQUAL(sizeof(T), (unsigned)4)
}

#if defined(WINNT)

BOOST_AUTO_TEST_CASE(BoostUnitTestShouldNotAppear3)
{
    BOOST_WARN(sizeof(int) == sizeof(short));
}

#else

BOOST_AUTO_TEST_CASE(BoostUnitTestConditional)
{
    BOOST_WARN(sizeof(int) == sizeof(short));
}

#endif

/*
BOOST_AUTO_TEST_CASE( BoostUnitTestShouldNotAppear4 )
{
BOOST_WARN( sizeof(int) == sizeof(short) );
}
*/