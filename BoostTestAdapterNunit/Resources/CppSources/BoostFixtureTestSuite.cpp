#include "stdafx.h"
#include <boost/test/test_case_template.hpp>
#include <boost/mpl/list.hpp>

class TestClassA
{
public:
    TestClassA()
    {
        m_testVar = 999;
    }
    ~TestClassA()
    {}

    int m_testVar;
};

class TestClassB
{
public:
    TestClassB()
    {
    }
    ~TestClassB()
    {}
};

BOOST_FIXTURE_TEST_SUITE(FixtureSuite1, TestClassA);

BOOST_AUTO_TEST_CASE(BoostTest1)
{
    BOOST_CHECK(m_testVar == 999);
}

BOOST_AUTO_TEST_CASE(BoostTest2)
{
    m_testVar = 0;
    BOOST_CHECK(m_testVar == 999);
}

BOOST_AUTO_TEST_SUITE_END();

BOOST_AUTO_TEST_CASE(BoostTest3)
{
    BOOST_TEST_MESSAGE("Outside the Fixture test Suite");
}

BOOST_FIXTURE_TEST_SUITE(FixtureSuite2, TestClassA);

BOOST_FIXTURE_TEST_CASE(Fixturetest_case1, TestClassB)
{
    BOOST_CHECK_EQUAL(1, 1);
}

typedef boost::mpl::list<int, long, char> type_list;

BOOST_AUTO_TEST_CASE_TEMPLATE(TemplatedTest, T, type_list)
{
    BOOST_CHECK_EQUAL(sizeof(T), (unsigned)4);
}
BOOST_AUTO_TEST_SUITE_END();