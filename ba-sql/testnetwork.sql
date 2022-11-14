--- MACADDROID

CREATE OR REPLACE FUNCTION returnMacAddress(my_address MACADDR) RETURNS MACADDR AS $$
return my_address;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR', 'returnMacAddress', returnMacAddress(MACADDR '08-00-2b-01-02-03') = MACADDR '08-00-2b-01-02-03';

CREATE OR REPLACE FUNCTION compareMacAddress(address1 MACADDR, address2 MACADDR) RETURNS BOOLEAN AS $$
return address1.Equals(address2);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR', 'compareMacAddress', compareMacAddress(MACADDR '08:00:2a:01:02:03', MACADDR '08-00-2b-01-02-03') is false;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR', 'compareMacAddress', compareMacAddress(MACADDR '08:00:2a:01:02:03', MACADDR '08-00-2a-01-02-03') is true;

--- MACADDR8OID

CREATE OR REPLACE FUNCTION addOneMacAddress8(my_address MACADDR8) RETURNS MACADDR8 AS $$
byte[] bytes = my_address.GetAddressBytes();
bytes[7] += 1;
return new PhysicalAddress(bytes);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR8', 'addOneMacAddress8', addOneMacAddress8(MACADDR8 '08:00:2b:01:02:03:04:05') = MACADDR8 '08:00:2b:01:02:03:04:06';

CREATE OR REPLACE FUNCTION compareMacAddress8(address1 MACADDR8, address2 MACADDR8) RETURNS BOOLEAN AS $$
return address1.Equals(address2);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR8', 'compareMacAddress8', compareMacAddress8(MACADDR8 '08:00:2b:01:02:03:04:06', MACADDR8 '08-00-2b-01-02-03-04-06') is true;

--- INETOID

CREATE OR REPLACE FUNCTION modifyNetMask(my_inet INET, n INT) RETURNS INET AS $$
return (my_inet.Address, my_inet.Netmask + n);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INET', 'modifyNetMask', modifyNetMask(INET '192.168.0.1/24', 6) = INET '192.168.0.1/30';

CREATE OR REPLACE FUNCTION modifyIP(my_inet INET, n INT) RETURNS INET AS $$
byte[] bytes = my_inet.Address.GetAddressBytes();
int size = bytes.Length;
bytes[size-1]+=(byte)n;
return (new IPAddress(bytes), my_inet.Netmask);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INET', 'modifyNetMask', modifyIP(INET '2001:db8:3333:4444:5555:6666:1.2.3.4/25', 20) = INET '2001:db8:3333:4444:5555:6666:1.2.3.24/25';

--- CIDROID

CREATE OR REPLACE FUNCTION modifyIP_CIDR(my_inet CIDR, pos INT, delta INT) RETURNS CIDR AS $$
byte[] bytes = my_inet.Address.GetAddressBytes();
bytes[pos]+=(byte)delta;
return (new IPAddress(bytes), my_inet.Netmask);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CIDR', 'modifyIP_CIDR', modifyIP_CIDR(CIDR '192.168/24', 0, 6) = CIDR '198.168.0.0/24';

CREATE OR REPLACE FUNCTION modifyNetmask_CIDR(my_inet CIDR, delta INT) RETURNS CIDR AS $$
return (my_inet.Address, my_inet.Netmask + delta);
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CIDR', 'modifyNetmask_CIDR', modifyNetmask_CIDR(CIDR '2001:4f8:3:ba::/64', 10) = CIDR '2001:4f8:3:ba::/74';

--- MACADDROID Arrays

CREATE OR REPLACE FUNCTION returnMacAddressArray(addresses MACADDR[]) RETURNS MACADDR[] AS $$
return addresses;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR[]', 'returnMacAddressArray1', returnMacAddressArray(ARRAY[[null::macaddr, null::macaddr], [null::macaddr, MACADDR 'a8-00-2b-01-02-03']]) = ARRAY[[null::macaddr, null::macaddr], [null::macaddr, MACADDR 'a8-00-2b-01-02-03']];

CREATE OR REPLACE FUNCTION updateArrayMacAddressIndex(values_array MACADDR[], desired MACADDR, index integer[]) RETURNS MACADDR[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR[]', 'updateArrayMacAddressIndex1', updateArrayMacAddressIndex(ARRAY[MACADDR '08-00-2b-01-02-03', MACADDR '09-00-2b-01-02-03', null::macaddr, MACADDR 'a8-00-2b-01-02-03'], MACADDR 'd1-00-2b-01-02-03', ARRAY[2]) = ARRAY[MACADDR '08-00-2b-01-02-03', MACADDR '09-00-2b-01-02-03', MACADDR 'd1-00-2b-01-02-03', MACADDR 'a8-00-2b-01-02-03'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR[]', 'updateArrayMacAddressIndex2', updateArrayMacAddressIndex(ARRAY[[MACADDR '08-00-2b-01-02-03', MACADDR '09-00-2b-01-02-03'], [null::macaddr, MACADDR 'a8-00-2b-01-02-03']], MACADDR 'd1-00-2b-01-02-03', ARRAY[1, 0]) = ARRAY[[MACADDR '08-00-2b-01-02-03', MACADDR '09-00-2b-01-02-03'], [MACADDR 'd1-00-2b-01-02-03', MACADDR 'a8-00-2b-01-02-03']];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR[]', 'updateArrayMacAddressIndex3', updateArrayMacAddressIndex(ARRAY[[null::macaddr, null::macaddr], [null::macaddr, MACADDR 'a8-00-2b-01-02-03']], MACADDR 'd1-00-2b-01-02-03', ARRAY[1, 0]) = ARRAY[[null::macaddr, null::macaddr], [MACADDR 'd1-00-2b-01-02-03', MACADDR 'a8-00-2b-01-02-03']];

CREATE OR REPLACE FUNCTION IncreaseMacAddress(values_array MACADDR[]) RETURNS MACADDR[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    PhysicalAddress orig_value = (PhysicalAddress)flatten_values.GetValue(i);
    byte[] bytes = orig_value.GetAddressBytes();
    bytes[0] += 1;
    PhysicalAddress new_value = new PhysicalAddress(bytes);
    
    flatten_values.SetValue((PhysicalAddress)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR[]', 'IncreaseMacAddress1', IncreaseMacAddress(ARRAY[MACADDR '08-00-2b-01-02-03', MACADDR '09-00-2b-01-02-03', null::macaddr, MACADDR 'a8-00-2b-01-02-03']) = ARRAY[MACADDR '09-00-2b-01-02-03', MACADDR '0a-00-2b-01-02-03', null::macaddr, MACADDR 'a9-00-2b-01-02-03'];


CREATE OR REPLACE FUNCTION CreateMacAddressMultidimensionalArray() RETURNS MACADDR[] AS $$
byte[] bytes = new byte[6] {171, 1, 43, 49, 65, 250};
PhysicalAddress objects_value = new PhysicalAddress(bytes);
PhysicalAddress?[, ,] three_dimensional_array = new PhysicalAddress?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR[]', 'CreateMacAddressMultidimensionalArray', CreateMacAddressMultidimensionalArray() = ARRAY[[[MACADDR 'ab-01-2b-31-41-fa', MACADDR 'ab-01-2b-31-41-fa'], [null::MACADDR, null::MACADDR]], [[MACADDR 'ab-01-2b-31-41-fa', null::MACADDR], [MACADDR 'ab-01-2b-31-41-fa', MACADDR 'ab-01-2b-31-41-fa']]];

--- MACADDR8OID Arrays

CREATE OR REPLACE FUNCTION updateArrayMacAddress8Index(values_array MACADDR8[], desired MACADDR8, index integer[]) RETURNS MACADDR8[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR8[]', 'updateArrayMacAddress8Index1', updateArrayMacAddress8Index(ARRAY[MACADDR8 '08-00-2b-01-02-03-ab-ac', MACADDR8 '09-00-2b-01-02-03-ab-ac', null::macaddr8, MACADDR8 'a8-00-2b-01-02-03-ab-ac'], MACADDR8 'd1-00-2b-01-02-03-ab-ac', ARRAY[2]) = ARRAY[MACADDR8 '08-00-2b-01-02-03-ab-ac', MACADDR8 '09-00-2b-01-02-03-ab-ac', MACADDR8 'd1-00-2b-01-02-03-ab-ac', MACADDR8 'a8-00-2b-01-02-03-ab-ac'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR8[]', 'updateArrayMacAddress8Index2', updateArrayMacAddress8Index(ARRAY[[MACADDR8 '08-00-2b-01-02-03-ab-ac', MACADDR8 '09-00-2b-01-02-03-ab-ac'], [null::macaddr8, MACADDR8 'a8-00-2b-01-02-03-ab-ac']], MACADDR8 'd1-00-2b-01-02-03-ab-ac', ARRAY[1, 0]) = ARRAY[[MACADDR8 '08-00-2b-01-02-03-ab-ac', MACADDR8 '09-00-2b-01-02-03-ab-ac'], [MACADDR8 'd1-00-2b-01-02-03-ab-ac', MACADDR8 'a8-00-2b-01-02-03-ab-ac']];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR8[]', 'updateArrayMacAddress8Index3', updateArrayMacAddress8Index(ARRAY[[null::MACADDR8, null::MACADDR8], [null::macaddr8, MACADDR8 'a8-00-2b-01-02-03-ab-ac']], MACADDR8 'd1-00-2b-01-02-03-ab-ac', ARRAY[1, 0]) = ARRAY[[null::MACADDR8, null::MACADDR8], [MACADDR8 'd1-00-2b-01-02-03-ab-ac', MACADDR8 'a8-00-2b-01-02-03-ab-ac']];

CREATE OR REPLACE FUNCTION IncreaseMacAddress8(values_array MACADDR8[]) RETURNS MACADDR8[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    PhysicalAddress orig_value = (PhysicalAddress)flatten_values.GetValue(i);
    byte[] bytes = orig_value.GetAddressBytes();
    bytes[0] += 1;
    PhysicalAddress new_value = new PhysicalAddress(bytes);
    
    flatten_values.SetValue((PhysicalAddress)new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR8[]', 'IncreaseMacAddress81', IncreaseMacAddress8(ARRAY[MACADDR8 '08-00-2b-01-02-03-ab-ac', MACADDR8 '09-00-2b-01-02-03-ab-ac', null::macaddr, MACADDR8 'a8-00-2b-01-02-03-ab-ac']) = ARRAY[MACADDR8 '09-00-2b-01-02-03-ab-ac', MACADDR8 '0a-00-2b-01-02-03-ab-ac', null::macaddr, MACADDR8 'a9-00-2b-01-02-03-ab-ac'];

CREATE OR REPLACE FUNCTION CreateMacAddress8MultidimensionalArray() RETURNS MACADDR8[] AS $$
byte[] bytes = new byte[8] {171, 1, 43, 49, 65, 250, 171, 172};
PhysicalAddress objects_value = new PhysicalAddress(bytes);
PhysicalAddress?[, ,] three_dimensional_array = new PhysicalAddress?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'MACADDR8[]', 'CreateMacAddress8MultidimensionalArray', CreateMacAddress8MultidimensionalArray() = ARRAY[[[MACADDR8 'ab-01-2b-31-41-fa-ab-ac', MACADDR8 'ab-01-2b-31-41-fa-ab-ac'], [null::MACADDR8, null::MACADDR8]], [[MACADDR8 'ab-01-2b-31-41-fa-ab-ac', null::MACADDR8], [MACADDR8 'ab-01-2b-31-41-fa-ab-ac', MACADDR8 'ab-01-2b-31-41-fa-ab-ac']]];

--- INETOID Arrays

CREATE OR REPLACE FUNCTION updateArrayNetMaskIndex(values_array INET[], desired INET, index integer[]) RETURNS INET[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INET[]', 'updateArrayNetMaskIndex1', updateArrayNetMaskIndex(ARRAY[INET '192.168.0.1/24', INET '192.170.0.1/24', null::inet, INET '170.168.0.1/24'], INET '192.168.0.120/24', ARRAY[2]) = ARRAY[INET '192.168.0.1/24', INET '192.170.0.1/24', INET '192.168.0.120/24', INET '170.168.0.1/24'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INET[]', 'updateArrayNetMaskIndex2', updateArrayNetMaskIndex(ARRAY[[INET '192.168.0.1/24', INET '192.170.0.1/24'], [null::inet, INET '170.168.0.1/24']], INET '192.168.0.120/24', ARRAY[1, 0]) = ARRAY[[INET '192.168.0.1/24', INET '192.170.0.1/24'], [INET '192.168.0.120/24', INET '170.168.0.1/24']];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INET[]', 'updateArrayNetMaskIndex3', updateArrayNetMaskIndex(ARRAY[[null::INET, null::INET], [null::inet, INET '170.168.0.1/24']], INET '192.168.0.120/24', ARRAY[1, 0]) = ARRAY[[null::INET, null::INET], [INET '192.168.0.120/24', INET '170.168.0.1/24']];

CREATE OR REPLACE FUNCTION IncreaseInetAddress(values_array INET[]) RETURNS INET[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    (IPAddress Address, int Netmask) orig_value = ((IPAddress Address, int Netmask))flatten_values.GetValue(i);
    byte[] bytes = orig_value.Address.GetAddressBytes();
    bytes[0] += 1;
    (IPAddress Address, int Netmask) new_value = (new IPAddress(bytes), orig_value.Netmask);
    
    flatten_values.SetValue(((IPAddress Address, int Netmask))new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INET[]', 'IncreaseInetAddress1', IncreaseInetAddress(ARRAY[INET '192.168.0.1/24', INET '192.170.0.1/24', null::inet, INET '170.168.0.1/24']) = ARRAY[INET '193.168.0.1/24', INET '193.170.0.1/24', null::inet, INET '171.168.0.1/24'];

CREATE OR REPLACE FUNCTION CreateInetMultidimensionalArray() RETURNS INET[] AS $$
(IPAddress Address, int Netmask) objects_value = (IPAddress.Parse("127.0.0.1"), 21);
(IPAddress Address, int Netmask)?[, ,] three_dimensional_array = new (IPAddress Address, int Netmask)?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'INET[]', 'CreateInetMultidimensionalArray', CreateInetMultidimensionalArray() = ARRAY[[[INET '127.0.0.1/21', INET '127.0.0.1/21'], [null::INET, null::INET]], [[INET '127.0.0.1/21', null::INET], [INET '127.0.0.1/21', INET '127.0.0.1/21']]];

--- CIDROID Arrays

CREATE OR REPLACE FUNCTION updateArrayCIDRIndex(values_array CIDR[], desired CIDR, index integer[]) RETURNS CIDR[] AS $$
int[] arrayInteger = index.Cast<int>().ToArray();
values_array.SetValue(desired, arrayInteger);
return values_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CIDR[]', 'updateArrayCIDRIndex1', updateArrayCIDRIndex(ARRAY[CIDR '192.168/24', CIDR '170.168/24', null::cidr, CIDR '142.168/24'], CIDR '192.169/24', ARRAY[2]) = ARRAY[CIDR '192.168/24', CIDR '170.168/24', CIDR '192.169/24', CIDR '142.168/24'];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CIDR[]', 'updateArrayCIDRIndex2', updateArrayCIDRIndex(ARRAY[[CIDR '192.168/24', CIDR '170.168/24'], [null::cidr, CIDR '142.168/24']], CIDR '192.169/24', ARRAY[1, 0]) = ARRAY[[CIDR '192.168/24', CIDR '170.168/24'], [CIDR '192.169/24', CIDR '142.168/24']];
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CIDR[]', 'updateArrayCIDRIndex3', updateArrayCIDRIndex(ARRAY[[null::CIDR, null::CIDR], [null::cidr, CIDR '142.168/24']], CIDR '192.169/24', ARRAY[1, 0]) = ARRAY[[null::CIDR, null::CIDR], [CIDR '192.169/24', CIDR '142.168/24']];


CREATE OR REPLACE FUNCTION IncreaseCIDRAddress(values_array CIDR[]) RETURNS CIDR[] AS $$
Array flatten_values = Array.CreateInstance(typeof(object), values_array.Length);
ArrayHandler.FlatArray(values_array, ref flatten_values);
for(int i = 0; i < flatten_values.Length; i++)
{   
    if (flatten_values.GetValue(i) == null)
        continue;

    (IPAddress Address, int Netmask) orig_value = ((IPAddress Address, int Netmask))flatten_values.GetValue(i);
    byte[] bytes = orig_value.Address.GetAddressBytes();
    bytes[0] += 1;
    (IPAddress Address, int Netmask) new_value = (new IPAddress(bytes), orig_value.Netmask);
    
    flatten_values.SetValue(((IPAddress Address, int Netmask))new_value, i);
}
return flatten_values;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CIDR[]', 'IncreaseCIDRAddress1', IncreaseCIDRAddress(ARRAY[CIDR '192.168.231.0/24', CIDR '175.170.14.0/24', null::cidr, CIDR '167.168.41.0/24']) = ARRAY[CIDR '193.168.231.0/24', CIDR '176.170.14.0/24', null::cidr, CIDR '168.168.41.0/24'];

CREATE OR REPLACE FUNCTION CreateCIDRMultidimensionalArray() RETURNS CIDR[] AS $$
(IPAddress Address, int Netmask) objects_value = (IPAddress.Parse("127.123.54.0"), 24);
(IPAddress Address, int Netmask)?[, ,] three_dimensional_array = new (IPAddress Address, int Netmask)?[2, 2, 2] {{{objects_value, objects_value}, {null, null}}, {{objects_value, null}, {objects_value, objects_value}}};
return three_dimensional_array;
$$ LANGUAGE plcsharp STRICT;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CIDR[]', 'CreateCIDRMultidimensionalArray', CreateCIDRMultidimensionalArray() = ARRAY[[[CIDR '127.123.54.0/24', CIDR '127.123.54.0/24'], [null::CIDR, null::CIDR]], [[CIDR '127.123.54.0/24', null::CIDR], [CIDR '127.123.54.0/24', CIDR '127.123.54.0/24']]];
