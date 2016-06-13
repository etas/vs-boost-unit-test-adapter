#define BOOST_TEST_MODULE DataTestCaseSample

#include <boost/test/included/unit_test.hpp>
#include <boost/test/data/test_case.hpp>

struct Fixture
{
    Fixture()
    {
        std::cout << "Fixtrure::Fixture()" << std::endl;
    };

    ~Fixture() noexcept
    {
        std::cout << "Fixtrure::~Fixture()" << std::endl;
    };
};

BOOST_AUTO_TEST_SUITE(data_test_suite)

BOOST_DATA_TEST_CASE(data_1, boost::unit_test::data::make({ 1, 2, 3 }))
{
    BOOST_CHECK_NE(sample, 4);
}

BOOST_DATA_TEST_CASE_F(Fixture, data_2, boost::unit_test::data::make({ 1, 2, 3 }))
{
    BOOST_FAIL("TODO: data_2");
}

BOOST_AUTO_TEST_SUITE_END()

BOOST_DATA_TEST_CASE(data_3, boost::unit_test::data::make({ 1, 2, 3 }), number)
{
    BOOST_CHECK_EQUAL(number, 1);
}