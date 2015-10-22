#include "stdafx.h"

struct F
{
    F() : i(0)
    {
        BOOST_TEST_MESSAGE("setup fixture");
    }
    ~F()
    {
        BOOST_TEST_MESSAGE("teardown fixture");
    }

    int i;
};

BOOST_AUTO_TEST_SUITE(Suit1)

BOOST_AUTO_TEST_CASE(BoostUnitTest1)
{
    BOOST_CHECK(1 == 1);
}

BOOST_FIXTURE_TEST_CASE(Fixturetest_case1, F)
{
    BOOST_CHECK(i == 1);
    ++i;
}

BOOST_FIXTURE_TEST_CASE(Fixturetest_case2, F)
{
    BOOST_CHECK_EQUAL(i, 1);
}

BOOST_AUTO_TEST_SUITE_END()

BOOST_FIXTURE_TEST_CASE(Fixturetest_case3, F)
{
    BOOST_CHECK_EQUAL(i, 1);
}