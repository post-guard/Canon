import os
import sys
from pathlib import Path


def get_test_pas_files():
    open_set_dir = Path("open_set")

    for file in open_set_dir.iterdir():
        if file.suffix == ".pas":
            yield file


def delete_generated_files():
    print("Info: deleting generated files...")
    open_set_dir = Path("open_set")

    for file in open_set_dir.iterdir():
        if file.suffix == ".out":
            os.system("rm " + str(file.resolve()))
        if file.suffix == ".c":
            os.system("rm " + str(file.resolve()))
        if file.suffix == ".c_result":
            os.system("rm " + str(file.resolve()))
        if file.suffix == ".pas_result":
            os.system("rm " + str(file.resolve()))


def compile_files():
    binary_files = []

    for file in get_test_pas_files():
        print("Info: compile ", file)
        if not (Path("open_set") / file.stem).exists():
            os.system("fpc " + str(file))
        os.system("./pacss -i " + str(file))
        c_file = "./open_set/" + file.stem + ".c"
        c_binary = "open_set/" + file.stem + ".out"
        os.system("gcc " + c_file + " -o " + c_binary)

        pascal_binary = "./open_set/" + file.stem
        input_file = Path("open_set") / (file.stem + ".in")
        if input_file.exists():
            pascal_binary = "cat " + str(input_file) + " | " + pascal_binary
            c_binary = "cat " + str(input_file) + " | " + c_binary

        binary_files.append((file.stem, pascal_binary, c_binary))

    return binary_files


def run_binary():
    binary_files = compile_files()
    for pair in binary_files:
        print("Info: run " + pair[0])

        pascal_result = "open_set/" + pair[0] + ".pas_result"
        c_result = "open_set/" + pair[0] + ".c_result"

        os.system(pair[1] + " > " + pascal_result)
        os.system(pair[2] + " > " + c_result)


def check_result():
    tests = []
    for file in Path("open_set").iterdir():
        if file.suffix != ".pas_result":
            continue
        tests.append(file.stem)
    tests = sorted(tests)

    pass_result = 0
    for test in tests:
        print("------Test " + test + "------")
        pascal_result = "open_set/" + test + ".pas_result"
        c_result = "open_set/" + test + ".c_result"

        with open(pascal_result) as pascal:
            with open(c_result) as c:
                pascal_result = pascal.readlines()
                c_result = c.readlines()

                flag = True
                if len(pascal_result) != len(c_result):
                    flag = False

                for j in range(0, len(pascal_result)):
                    if not flag:
                        break

                    flag = c_result[j] == pascal_result[j]

                if flag:
                    print("test " + test + " passed!")
                    pass_result += 1
                else:
                    print("test " + test + " failed!")
                    print("Pascal: ", pascal_result)
                    print("C: ", c_result)

    print(str(pass_result) + "/" + str(len(tests)) + " tests passed!")


if __name__ == "__main__":
    if sys.argv[1] == "run":
        delete_generated_files()
        run_binary()
    elif sys.argv[1] == "test":
        check_result()
