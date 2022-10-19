--- MACADDROID

CREATE OR REPLACE FUNCTION returnMacAddress(my_address MACADDR) RETURNS MACADDR AS $$
return my_address;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR', 'returnMacAddress', returnMacAddress(MACADDR '08-00-2b-01-02-03') = MACADDR '08-00-2b-01-02-03';

CREATE OR REPLACE FUNCTION compareMacAddress(address1 MACADDR, address2 MACADDR) RETURNS BOOLEAN AS $$
return address1.Equals(address2);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR', 'compareMacAddress', compareMacAddress(MACADDR '08:00:2a:01:02:03', MACADDR '08-00-2b-01-02-03') is false;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR', 'compareMacAddress', compareMacAddress(MACADDR '08:00:2a:01:02:03', MACADDR '08-00-2a-01-02-03') is true;

--- MACADDR8OID

CREATE OR REPLACE FUNCTION addOneMacAddress8(my_address MACADDR8) RETURNS MACADDR8 AS $$
byte[] bytes = my_address.GetAddressBytes();
bytes[7] += 1;
return new PhysicalAddress(bytes);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR8', 'addOneMacAddress8', addOneMacAddress8(MACADDR8 '08:00:2b:01:02:03:04:05') = MACADDR8 '08:00:2b:01:02:03:04:06';

CREATE OR REPLACE FUNCTION compareMacAddress8(address1 MACADDR8, address2 MACADDR8) RETURNS BOOLEAN AS $$
return address1.Equals(address2);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR8', 'compareMacAddress8', compareMacAddress8(MACADDR8 '08:00:2b:01:02:03:04:06', MACADDR8 '08-00-2b-01-02-03-04-06') is true;

--- INETOID

CREATE OR REPLACE FUNCTION modifyNetMask(my_inet INET, n INT) RETURNS INET AS $$
return (my_inet.Address, my_inet.Netmask + n);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INET', 'modifyNetMask', modifyNetMask(INET '192.168.0.1/24', 6) = INET '192.168.0.1/30';

CREATE OR REPLACE FUNCTION modifyIP(my_inet INET, n INT) RETURNS INET AS $$
byte[] bytes = my_inet.Address.GetAddressBytes();
int size = bytes.Length;
bytes[size-1]+=(byte)n;
return (new IPAddress(bytes), my_inet.Netmask);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INET', 'modifyNetMask', modifyIP(INET '2001:db8:3333:4444:5555:6666:1.2.3.4/25', 20) = INET '2001:db8:3333:4444:5555:6666:1.2.3.24/25';

--- CIDROID

CREATE OR REPLACE FUNCTION modifyIP_CIDR(my_inet CIDR, pos INT, delta INT) RETURNS CIDR AS $$
byte[] bytes = my_inet.Address.GetAddressBytes();
bytes[pos]+=(byte)delta;
return (new IPAddress(bytes), my_inet.Netmask);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CIDR', 'modifyIP_CIDR', modifyIP_CIDR(CIDR '192.168/24', 0, 6) = CIDR '198.168.0.0/24';

CREATE OR REPLACE FUNCTION modifyNetmask_CIDR(my_inet CIDR, delta INT) RETURNS CIDR AS $$
return (my_inet.Address, my_inet.Netmask + delta);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CIDR', 'modifyNetmask_CIDR', modifyNetmask_CIDR(CIDR '2001:4f8:3:ba::/64', 10) = CIDR '2001:4f8:3:ba::/74';