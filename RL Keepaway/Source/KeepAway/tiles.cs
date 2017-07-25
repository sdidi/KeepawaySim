using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keepaway
{
    class tiles
    {
        public static int max_num_vars = 64;
        public static int max_num_coords = 1000;
        public static int max_longInt = 2147483647;

        int mod(int n, int k) {return (n >= 0) ? n%k : k-1-((-n-1)%k);}

   public void GetTiles(
    int []tiles,               // provided array contains returned tiles (tile indices)
    int num_tilings,           // number of tile indices to be returned in tiles
    int memory_size,           // total number of possible tiles
    float []floats,            // array of floating point variables
    int num_floats,            // number of floating point variables
    int []ints,       // array of integer variables
    int num_ints)              // number of integer variables
{
    int i,j;
    int [] qstate = new int[max_num_vars];
    int [] baseDim= new int[max_num_vars];
    int [] coordinates= new int[max_num_vars * 2 + 1];   /* one interval number per relevant dimension */
    int num_coordinates = num_floats + num_ints + 1;

    for (int f=0; f<num_ints; f++)
        coordinates[num_floats+1+f] = ints[f];

    /* quantize state to integers (henceforth, tile widths == num_tilings) */
    for (i = 0; i < num_floats; i++)
    {
        qstate[i] = (int) Math.Floor(floats[i] * num_tilings);
        baseDim[i] = 0;
    }

    /*compute the tile numbers */
    for (j = 0; j < num_tilings; j++)
    {

        /* loop over each relevant dimension */
        for (i = 0; i < num_floats; i++)
        {

            /* find coordinates of activated tile in tiling space */
            coordinates[i] = qstate[i] - mod(qstate[i]-baseDim[i],num_tilings);

            /* compute displacement of next tiling in quantized space */
            baseDim[i] += 1 + (2 * i);
        }
        /* add additional indices for tiling and hashing_set so they hash differently */
        coordinates[i] = j;
        Console.WriteLine("Number of cordinates = {0}", num_coordinates);
        tiles[j] = hash_UNH(coordinates, num_coordinates, memory_size, 449);
    }
    return;
}


public void GetTiles(
    int []tiles,               // provided array contains returned tiles (tile indices)
    int num_tilings,           // number of tile indices to be returned in tiles
    collision_table ctable,    // total number of possible tiles
    float [] floats,            // array of floating point variables
    int num_floats,            // number of floating point variables
    int []ints,       // array of integer variables
    int num_ints)              // number of integer variables
{
    int i,j;
    int  [] qstate = new int[max_num_vars];
    int [] baseDim= new int[max_num_vars];
    int [] coordinates= new int[max_num_vars * 2 + 1];   /* one interval number per relevant dimension */
    int num_coordinates = num_floats + num_ints + 1;

    for (int f=0; f<num_ints; f++)
        coordinates[num_floats+1+f] = ints[f];

    /* quantize state to integers (henceforth, tile widths == num_tilings) */
    for (i = 0; i < num_floats; i++)
    {
        qstate[i] = (int) Math.Floor(floats[i] * num_tilings);
        baseDim[i] = 0;
    }

    /*compute the tile numbers */
    for (j = 0; j < num_tilings; j++)
    {

        /* loop over each relevant dimension */
        for (i = 0; i < num_floats; i++)
        {

            /* find coordinates of activated tile in tiling space */
            coordinates[i] = qstate[i] - mod(qstate[i]-baseDim[i],num_tilings);

            /* compute displacement of next tiling in quantized space */
            baseDim[i] += 1 + (2 * i);
        }
        /* add additional indices for tiling and hashing_set so they hash differently */
        coordinates[i] = j;
        int []tiless = new int[num_tilings];
        tiless[j] = hash(coordinates, num_coordinates,ctable);
        tiles[j] = hash(coordinates, num_coordinates,ctable);
    }
    return;
}


/* hash_UNH
   Takes an array of integers and returns the corresponding tile after hashing 
*/

    static int []rndseq = new int[2048];
    static int first_call =  1;

int hash_UNH(int []ints, int num_ints, int m, int increment)
{
    
    int i,k;
    int index;
    int sum = 0;
    Random rand = new Random();
    /* if first call to hashing, initialize table of random numbers */
    if (first_call==1)
    {
        for (k = 0; k < 2048; k++)
        {
            rndseq[k] = 0;
            for (i=0; i < (int)sizeof(int); ++i)
                rndseq[k] = (rndseq[k] << 8) | (rand.Next() & 0xff);
        }
        first_call = 0;
    }

    for (i = 0; i < num_ints; i++)
    {
        /* add random table offset for this dimension and wrap around */
        index = ints[i];
        index += (increment * i);
        index %= 2048;
        while (index < 0)
            index += 2048;

        /* add selected random number to sum */
        sum += rndseq[(int)index];
    }
    index = (int)(sum % m);
    while (index < 0)
        index += m;

    return(index);
}


/* hash
   Takes an array of integers and returns the corresponding tile after hashing 
*/
int hash(int []ints, int num_ints, collision_table ct)
{
    int j;
    int ccheck;

    ct.calls++;
    j = hash_UNH(ints, num_ints, ct.m, 449);
    ccheck = hash_UNH(ints, num_ints, max_longInt, 457);
    if (ccheck == (int)ct.data[j])
        ct.clearhits++;
    else if (ct.data[j] == -1)
    {
        ct.clearhits++;
        ct.data[j] = ccheck;
    }
    else if (ct.safe == 0)
        ct.collisions++;
    else
    {
        int h2 = 1 + 2 * hash_UNH(ints,num_ints,(max_longInt)/4,449);
        int i = 0;
        while (++i>0)
        {
            ct.collisions++;
            j = (j+h2) % (ct.m);
            //printf("(%d)",j);
            if (i > ct.m)
            {
                Console.WriteLine("\nOut of Memory");
                Environment.Exit(0);
            }
            if (ccheck == ct.data[j])
                break;
            if (ct.data[j] == -1)
            {
                ct.data[j] = ccheck;
                break;
            }
        }
    }
    return j;
}



/*
void collision_table::save(char *filename) {
 write(open(filename, O_BINARY | O_CREAT | O_WRONLY);
};
    
void collision_table::restore(char *filename) {
 read(open(filename, O_BINARY | O_CREAT | O_WRONLY);
}
*/


int[] i_tmp_arr=new int[max_num_vars];
float [] f_tmp_arr= new float[max_num_vars];

// No ints
public void GetTiles(int []tiles, int nt,int memory,float [] floats,int nf)
{
    GetTiles(tiles,nt,memory,floats,nf,i_tmp_arr,0);
}
public void GetTiles(int []tiles,int nt,collision_table ct,float []floats,int nf)
{
    GetTiles(tiles,nt,ct,floats,nf,i_tmp_arr,0);
}

//one int
public void GetTiles(int []tiles,int nt,int memory,float []floats,int nf,int h1)
{
    i_tmp_arr[0]=h1;
    GetTiles(tiles,nt,memory,floats,nf,i_tmp_arr,1);
}
public void GetTiles(int []tiles,int nt,collision_table ct,float []floats,int nf,int h1)
{
    i_tmp_arr[0]=h1;
    GetTiles(tiles,nt,ct,floats,nf,i_tmp_arr,1);
}

// two ints
public void GetTiles(int []tiles,int nt,int memory,float []floats,int nf,int h1,int h2)
{
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    GetTiles(tiles,nt,memory,floats,nf,i_tmp_arr,2);
}
public void GetTiles(int []tiles,int nt,collision_table ct,float []floats,int nf,int h1,int h2)
{
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    GetTiles(tiles,nt,ct,floats,nf,i_tmp_arr,2);
}

// three ints
public void GetTiles(int []tiles,int nt,int memory,float []floats,int nf,int h1,int h2,int h3)
{
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    i_tmp_arr[2]=h3;
    GetTiles(tiles,nt,memory,floats,nf,i_tmp_arr,3);
}
public void GetTiles(int []tiles,int nt,collision_table ct,float []floats,int nf,int h1,int h2,int h3)
{
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    i_tmp_arr[2]=h3;
    GetTiles(tiles,nt,ct,floats,nf,i_tmp_arr,3);
}

// one float, No ints
public void GetTiles1(int []tiles,int nt,int memory,float f1)
{
    f_tmp_arr[0]=f1;
    GetTiles(tiles,nt,memory,f_tmp_arr,1,i_tmp_arr,0);
}
public void GetTiles1(int []tiles,int nt,collision_table ct,float f1)
{
    f_tmp_arr[0]=f1;
    GetTiles(tiles,nt,ct,f_tmp_arr,1,i_tmp_arr,0);
}

// one float, one int
public void GetTiles1(int []tiles,int nt,int memory,float f1,int h1)
{
    f_tmp_arr[0]=f1;
    i_tmp_arr[0]=h1;
    GetTiles(tiles,nt,memory,f_tmp_arr,1,i_tmp_arr,1);
}
public void GetTiles1(int []tiles,int nt,collision_table ct,float f1,int h1)
{
    f_tmp_arr[0]=f1;
    i_tmp_arr[0]=h1;
    GetTiles(tiles,nt,ct,f_tmp_arr,1,i_tmp_arr,1);
}

// one float, two ints
public void GetTiles1(int []tiles,int nt,int memory,float f1,int h1,int h2)
{
    f_tmp_arr[0]=f1;
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    GetTiles(tiles,nt,memory,f_tmp_arr,1,i_tmp_arr,2);
}
public void GetTiles1(int []tiles,int nt,collision_table ct,float f1,int h1,int h2)
{
    f_tmp_arr[0]=f1;
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    GetTiles(tiles,nt,ct,f_tmp_arr,1,i_tmp_arr,2);
}

// one float, three ints
public void GetTiles1(int []tiles,int nt,int memory,float f1,int h1,int h2,int h3)
{
    f_tmp_arr[0]=f1;
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    i_tmp_arr[2]=h3;
    GetTiles(tiles,nt,memory,f_tmp_arr,1,i_tmp_arr,3);
}
public void GetTiles1(int []tiles,int nt,collision_table ct,float f1,int h1,int h2,int h3)
{
    f_tmp_arr[0]=f1;
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    i_tmp_arr[2]=h3;
    GetTiles(tiles,nt,ct,f_tmp_arr,1,i_tmp_arr,3);
}

// two floats, No ints
public void GetTiles2(int []tiles,int nt,int memory,float f1,float f2)
{
    f_tmp_arr[0]=f1;
    f_tmp_arr[1]=f2;
    GetTiles(tiles,nt,memory,f_tmp_arr,2,i_tmp_arr,0);
}
public void GetTiles2(int []tiles,int nt,collision_table ct,float f1,float f2)
{
    f_tmp_arr[0]=f1;
    f_tmp_arr[1]=f2;
    GetTiles(tiles,nt,ct,f_tmp_arr,2,i_tmp_arr,0);
}

// two floats, one int
public void GetTiles2(int []tiles,int nt,int memory,float f1,float f2,int h1)
{
    f_tmp_arr[0]=f1;
    f_tmp_arr[1]=f2;
    i_tmp_arr[0]=h1;
    GetTiles(tiles,nt,memory,f_tmp_arr,2,i_tmp_arr,1);
}
public void GetTiles2(int []tiles,int nt,collision_table ct,float f1,float f2,int h1)
{
    f_tmp_arr[0]=f1;
    f_tmp_arr[1]=f2;
    i_tmp_arr[0]=h1;
    GetTiles(tiles,nt,ct,f_tmp_arr,2,i_tmp_arr,1);
}

// two floats, two ints
public void GetTiles2(int []tiles,int nt,int memory,float f1,float f2,int h1,int h2)
{
    f_tmp_arr[0]=f1;
    f_tmp_arr[1]=f2;
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    GetTiles(tiles,nt,memory,f_tmp_arr,2,i_tmp_arr,2);
}
public void GetTiles2(int []tiles,int nt,collision_table ct,float f1,float f2,int h1,int h2)
{
    f_tmp_arr[0]=f1;
    f_tmp_arr[1]=f2;
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    GetTiles(tiles,nt,ct,f_tmp_arr,2,i_tmp_arr,2);
}

// two floats, three ints
public void GetTiles2(int []tiles,int nt,int memory,float f1,float f2,int h1,int h2,int h3)
{
    f_tmp_arr[0]=f1;
    f_tmp_arr[1]=f2;
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    i_tmp_arr[2]=h3;
    GetTiles(tiles,nt,memory,f_tmp_arr,2,i_tmp_arr,3);
}
public void GetTiles2(int []tiles,int nt,collision_table ct,float f1,float f2,int h1,int h2,int h3)
{
    f_tmp_arr[0]=f1;
    f_tmp_arr[1]=f2;
    i_tmp_arr[0]=h1;
    i_tmp_arr[1]=h2;
    i_tmp_arr[2]=h3;
    GetTiles(tiles,nt,ct,f_tmp_arr,2,i_tmp_arr,3);
}

public void GetTilesWrap(
    int [] tiles,               // provided array contains returned tiles (tile indices)
    int num_tilings,           // number of tile indices to be returned in tiles
    int memory_size,           // total number of possible tiles
    float [] floats,            // array of floating point variables
    int num_floats,            // number of floating point variables
    int [] wrap_widths,         // array of widths (length and units as in floats)
    int [] ints,      // array of integer variables
    int num_ints)             // number of integer variables
{
    int i,j;
    int []qstate =new int[max_num_vars];
    int [] baseDim= new int[max_num_vars];
    int [] wrap_widths_times_num_tilings = new int[max_num_vars];
    int [] coordinates= new int[max_num_vars * 2 + 1];   /* one interval number per relevant dimension */
    int num_coordinates = num_floats + num_ints + 1;

    for ( i=0; i<num_ints; i++)
        coordinates[num_floats+1+i] = ints[i];

    /* quantize state to integers (henceforth, tile widths == num_tilings) */
    for (i = 0; i < num_floats; i++)
    {
        qstate[i] = (int) Math.Floor(floats[i] * num_tilings);
        baseDim[i] = 0;
        wrap_widths_times_num_tilings[i] = wrap_widths[i] * num_tilings;
    }

    /*compute the tile numbers */
    for (j = 0; j < num_tilings; j++)
    {

        /* loop over each relevant dimension */
        for (i = 0; i < num_floats; i++)
        {

            /* find coordinates of activated tile in tiling space */
            coordinates[i] = qstate[i] - mod(qstate[i]-baseDim[i],num_tilings);
            if (wrap_widths[i]!=0)
                coordinates[i] = coordinates[i] % wrap_widths_times_num_tilings[i];
            /* compute displacement of next tiling in quantized space */
            baseDim[i] += 1 + (2 * i);
        }
        /* add additional indices for tiling and hashing_set so they hash differently */
        coordinates[i] = j;

        tiles[j] = hash_UNH(coordinates, num_coordinates, memory_size, 449);
    }
    return;
}

public void GetTilesWrap(
    int []tiles,               // provided array contains returned tiles (tile indices)
    int num_tilings,           // number of tile indices to be returned in tiles
    collision_table ctable,   // total number of possible tiles
    float []floats,            // array of floating point variables
    int num_floats,            // number of floating point variables
    int []wrap_widths,         // array of widths (length and units as in floats)
    int []ints,                // array of integer variables
    int num_ints)              // number of integer variables
{
    int i,j;
    int []qstate = new int[max_num_vars];
    int []baseDim= new int[max_num_vars];
    int []wrap_widths_times_num_tilings= new int[max_num_vars];
    int []coordinates=new int[max_num_vars * 2 + 1];   /* one interval number per relevant dimension */
    int num_coordinates = num_floats + num_ints + 1;

    for ( i=0; i<num_ints; i++)
        coordinates[num_floats+1+i] = ints[i];

    /* quantize state to integers (henceforth, tile widths == num_tilings) */
    for (i = 0; i < num_floats; i++)
    {
        qstate[i] = (int) Math.Floor(floats[i] * num_tilings);
        baseDim[i] = 0;
        wrap_widths_times_num_tilings[i] = wrap_widths[i] * num_tilings;
    }

    /*compute the tile numbers */
    for (j = 0; j < num_tilings; j++)
    {

        /* loop over each relevant dimension */
        for (i = 0; i < num_floats; i++)
        {

            /* find coordinates of activated tile in tiling space */
            coordinates[i] = qstate[i] - mod(qstate[i]-baseDim[i],num_tilings);

            if (wrap_widths[i]!=0)
                coordinates[i] = mod(coordinates[i], wrap_widths_times_num_tilings[i]);
            /* compute displacement of next tiling in quantized space */
            baseDim[i] += 1 + (2 * i);
        }
        /* add additional indices for tiling and hashing_set so they hash differently */
        coordinates[i] = j;

        tiles[j] = hash(coordinates, num_coordinates,ctable);
    }
    return;
}
// no ints
public void GetTilesWrap(int []tiles,int num_tilings,int memory_size,float []floats,
                  int num_floats,int []wrap_widths)
{
    GetTilesWrap(tiles,num_tilings,memory_size,floats,
                 num_floats,wrap_widths,i_tmp_arr,0);
}



    }
}


