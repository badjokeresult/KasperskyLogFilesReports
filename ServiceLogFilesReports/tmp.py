#!/usr/bin/env python3


from random import randint


def main():
	result = ""
	for i in range(4):
		result += str(randint(1, 255)) + "."
	print(result[:-1])


if __name__ == "__main__":
	main()

