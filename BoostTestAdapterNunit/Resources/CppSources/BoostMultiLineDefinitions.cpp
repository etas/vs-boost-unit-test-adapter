#include "stdafx.h"

namespace {

struct F
{
    F() : i(0)
    {
        BOOST_TEST_MESSAGE("setup fixture");
    };

    ~F()
    {
        BOOST_TEST_MESSAGE("teardown fixture");
    };

    int i;
};

} // namespace (anonymous)

BOOST_AUTO_TEST_SUITE(
    AutoSuite
)

BOOST_AUTO_TEST_CASE(TestA)
{
    BOOST_CHECK(i == 0);
}

BOOST_FIXTURE_TEST_CASE(TestB, F)
{
    BOOST_CHECK(i == 1);
    ++i;
}

BOOST_FIXTURE_TEST_CASE(
    TestC, F
    
    )
{
    BOOST_CHECK_EQUAL(i, 1);
}

BOOST_FIXTURE_TEST_SUITE(
    FixtureSuite,
    F
)

BOOST_AUTO_TEST_CASE
(
    TestD
)
{
    BOOST_CHECK_NE(2, 1);
}

BOOST_AUTO_TEST_SUITE_END() // FixtureSuite
BOOST_AUTO_TEST_SUITE_END() // AutoSuite