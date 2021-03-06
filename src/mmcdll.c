/*
mmcdll.c - DLL wrapper for MMC

Copyright (c) 2015 sada.gussy at gmail dot com

This software is provided 'as-is', without any express or implied
warranty. In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

   1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.

   2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.

   3. This notice may not be removed or altered from any source
   distribution.
*/
#include "mmc.h"
#include "dlog.h"
#include <string.h>
/*---------------------------------------------------
 *---------------------------------------------------*/
__declspec(dllexport) void __cdecl mmc_version(char *version){
  strcpy(version, "mmc.dll version " MMC_VERSION);
}

/*---------------------------------------------------
 *---------------------------------------------------*/
__declspec(dllexport) int __cdecl mmc_convert(char *infilename, char *outfilename, char *console){
  dlog_clear();
  mmc_t *m = mmc_create();
  int result = mmc_parse_mml_file(m, infilename, outfilename);
  if(result == 0){
    dlog_add("Done successfully.");
  }else{
    dlog_add("Midi convert failed.");
  }
  strcpy(console, dlog_get());
  mmc_destroy(m);
  return result;
}

/*---------------------------------------------------
 *---------------------------------------------------*/
__declspec(dllexport) int __cdecl mmc_convert_string(char *mml, char *outfilename, char *console){
  dlog_clear();
  mmc_t *m = mmc_create();
  int result = mmc_parse_mml_string(m, mml, outfilename);
  if(m->error == 0){
    dlog_add("Convert done successfully.");
  }else{
    dlog_add("Convert failed.");
  }
  strcpy(console, dlog_get());
  mmc_destroy(m);
  return result;
}

