"""python3 -m unittest discover -s tools/builddata"""
import pathlib
import sys
import unittest

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
from builddata import PathError, eval_path, is_path, member_name, number, parse_ini  # noqa: E402
import gen  # noqa: E402


class EvalPath(unittest.TestCase):
    def test_parts_add_up(self):
        self.assertEqual(eval_path("Global_4718592.f_3838"), 4718592 + 3838)
        self.assertEqual(eval_path("uLocal_8883"), 8883)
        self.assertEqual(eval_path("f_795"), 795)
        self.assertEqual(eval_path("1574589.f_2"), 1574591)

    def test_every_index_counts_the_size_slot(self):
        self.assertEqual(eval_path("Global_4980736.f_71084[i /*631*/].f_14"), 4980736 + 71084 + 1 + 14)
        self.assertEqual(eval_path("Global_4980736.f_71084[i /*631*/].f_14[i]"), 4980736 + 71084 + 1 + 14 + 1)

    def test_a_number_as_index_is_part_of_the_address(self):
        self.assertEqual(eval_path("Global_4980736.f_49494[i /*53*/].f_6[1 /*11*/].f_2"),
                         4980736 + 49494 + 1 + 6 + 1 + 11 + 2)
        # Without a stride comment an element is one slot.
        self.assertEqual(eval_path("Global_4718592.f_112623[2]"), 4718592 + 112623 + 1 + 2)

    def test_not_paths(self):
        for v in ("0x24", "8,30,50", "48 8B 05 ? ? ? ?", "abc"):
            self.assertFalse(is_path(v), v)
        with self.assertRaises(PathError):
            eval_path("Global_1.foo")

    def test_numbers(self):
        self.assertEqual(number("0x1C"), 0x1C)
        self.assertEqual(number("-0x24"), -0x24)
        self.assertEqual(number("36"), 36)


class Ini(unittest.TestCase):
    def test_first_key_wins_and_comments_go(self):
        ini = parse_ini('[OFFSETS]\nOFFSET_a = "Global_1.f_2";compare Global_3\nOFFSET_A = "Global_9"\n')
        self.assertEqual(ini[("OFFSETS", "offset_a")], "Global_1.f_2")

    def test_member_names(self):
        self.assertEqual(member_name("OFFSET_actor_model"), "actor_model")
        self.assertEqual(member_name("Offset_ivm"), "ivm")
        self.assertEqual(member_name("worldptr"), "worldptr")


class Generated(unittest.TestCase):
    def test_generated_files_are_current(self):
        for name, text in gen.outputs().items():
            path = gen.OUT / name
            self.assertTrue(path.is_file(), name)
            self.assertEqual(path.read_text(encoding="utf-8-sig"), text,
                             f"{name} is stale: run python3 tools/builddata/gen.py")


if __name__ == "__main__":
    unittest.main()
