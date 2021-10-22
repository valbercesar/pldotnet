make installcheck
TEST_RESULTS_DIR=/app/pldotnet/debian/test_results/$(lsb_release -a | grep -Po '(?<=Release:\t)\d+(?=\.)')/
if [ -f /app/pldotnet/regression.diffs ]; then
    echo "Tests for this version did not achieve 100%. Storing results in debian/test_results/<OS Version>/"
    mkdir -p ${TEST_RESULTS_DIR}
    cp /app/pldotnet/regression.diffs ${TEST_RESULTS_DIR}
    cp /app/pldotnet/regression.out ${TEST_RESULTS_DIR}
fi
